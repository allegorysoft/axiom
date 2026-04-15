using System.Collections.Generic;
using Allegory.Axiom.Filter.Concrete;
using Allegory.Axiom.Filter.Enums;
using Allegory.Axiom.Filter.Tests.Models;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Filter.Tests;

public class ConditionExpressionExtensionTests
{
    [Fact]
    public void IsEquals()
    {
        var conditions = new Condition("column1", Operator.Equals, "some value");

        var expression = conditions.ToLambdaExpression<Sample>();

        expression.Invoke(new Sample
        {
            column1 = "some value"
        }).ShouldBeTrue();
    }

    [Fact]
    public void DoesntEquals()
    {
        var conditions = new Condition("column2", Operator.DoesntEquals, 10);

        var expression = conditions.ToLambdaExpression<Sample>();

        expression.Invoke(new Sample
        {
            column2 = 15
        }).ShouldBeTrue();
    }

    [Fact]
    public void GreaterThan()
    {
        var conditions = new Condition("column2", Operator.IsGreaterThan, 10);

        var expression = conditions.ToLambdaExpression<Sample>();

        expression.Invoke(new Sample
        {
            column2 = 15
        }).ShouldBeTrue();
    }

    [Fact]
    public void GreaterThanOrEqual()
    {
        var conditions = new Condition("column2", Operator.IsGreaterThanOrEqualto, 10);

        var expression = conditions.ToLambdaExpression<Sample>();

        expression.Invoke(new Sample
        {
            column2 = 10
        }).ShouldBeTrue();
    }

    [Fact]
    public void LessThan()
    {
        var conditions = new Condition("column2", Operator.IsLessThan, 10);

        var expression = conditions.ToLambdaExpression<Sample>();

        expression.Invoke(new Sample
        {
            column2 = 5
        }).ShouldBeTrue();
    }

    [Fact]
    public void LessThanOrEqual()
    {
        var conditions = new Condition("column2", Operator.IsLessThanOrEqualto, 10);

        var expression = conditions.ToLambdaExpression<Sample>();

        expression.Invoke(new Sample
        {
            column2 = 10
        }).ShouldBeTrue();
    }

    [Fact]
    public void Between()
    {
        var conditions = new Condition("column2", Operator.IsBetween, new int[]
        {
            10, 15
        });

        var expression = conditions.ToLambdaExpression<Sample>();

        expression.Invoke(new Sample
        {
            column2 = 12
        }).ShouldBeTrue();
    }

    [Fact]
    public void Contains()
    {
        var conditions = new Condition("column1", Operator.Contains, "thin");

        var expression = conditions.ToLambdaExpression<Sample>();

        expression.Invoke(new Sample
        {
            column1 = "something"
        }).ShouldBeTrue();
        expression.Invoke(new Sample
        {
            column1 = "emtpy"
        }).ShouldBeFalse();
    }

    [Fact]
    public void StartsWith()
    {
        var conditions = new Condition("column1", Operator.StartsWith, "some");

        var expression = conditions.ToLambdaExpression<Sample>();

        expression.Invoke(new Sample
        {
            column1 = "something"
        }).ShouldBeTrue();
        expression.Invoke(new Sample
        {
            column1 = "emtpy"
        }).ShouldBeFalse();
    }

    [Fact]
    public void EndsWith()
    {
        var conditions = new Condition("column1", Operator.EndsWith, "ing");

        var expression = conditions.ToLambdaExpression<Sample>();

        expression.Invoke(new Sample
        {
            column1 = "something"
        }).ShouldBeTrue();
        expression.Invoke(new Sample
        {
            column1 = "emtpy"
        }).ShouldBeFalse();
    }

    [Fact]
    public void IsNull()
    {
        var conditions = new Condition("column1", Operator.IsNull);

        var expression = conditions.ToLambdaExpression<Sample>();

        expression.Invoke(new Sample
        {
            column1 = null
        }).ShouldBeTrue();
    }

    [Fact]
    public void IsNullOrEmpty()
    {
        var conditions = new Condition("column1", Operator.IsNullOrEmpty);

        var expression = conditions.ToLambdaExpression<Sample>();

        expression.Invoke(new Sample
        {
            column1 = string.Empty
        }).ShouldBeTrue();
    }

    [Fact]
    public void In()
    {
        var conditions = new Condition("column2", Operator.In, new List<int>
        {
            10,
            12,
            14
        });

        var expression = conditions.ToLambdaExpression<Sample>();

        expression.Invoke(new Sample
        {
            column2 = 12
        }).ShouldBeTrue();
        expression.Invoke(new Sample
        {
            column2 = 13
        }).ShouldBeFalse();
    }

    [Fact]
    public void Group()
    {
        var conditions = new Condition
        {
            Group = new List<Condition>
            {
                new Condition
                {
                    Column = "column1",
                    Operator = Operator.Equals,
                    Value = "some value"
                },
                new Condition
                {
                    Column = "column2",
                    Operator = Operator.IsGreaterThan,
                    Value = 20
                },
                new Condition
                {
                    Column = "column3",
                    Operator = Operator.IsLessThan,
                    Value = 20
                }
            }
        };

        var expression = conditions.ToLambdaExpression<Sample>();

        expression.Invoke(new Sample
        {
            column1 = "some value",
            column2 = 25,
            column3 = 14
        }).ShouldBeTrue();
    }

    [Fact]
    public void GroupOr()
    {
        var conditions = new Condition
        {
            Group = new List<Condition>
            {
                new Condition
                {
                    Column = "column1",
                    Operator = Operator.Equals,
                    Value = "some value"
                },
                new Condition
                {
                    Column = "column2",
                    Operator = Operator.IsGreaterThan,
                    Value = 20
                }
            }
        };

        var expression = conditions.ToLambdaExpression<Sample>();

        expression.Invoke(new Sample
        {
            column1 = "some value",
            column2 = 3
        }).ShouldBeFalse();

        conditions.GroupOr = true;
        expression = conditions.ToLambdaExpression<Sample>();
        expression.Invoke(new Sample
        {
            column1 = "some value",
            column2 = 3
        }).ShouldBeTrue();
    }

    [Fact]
    public void Not()
    {
        var conditions = new Condition("column3", Operator.Equals, 10, true);

        var expression = conditions.ToLambdaExpression<Sample>();

        expression.Invoke(new Sample
        {
            column2 = 12
        }).ShouldBeTrue();
    }

    [Fact]
    public void ArgumentException()
    {
        var conditions = new Condition();

        Should.Throw<FilterException>(() =>
        {
            conditions.ToLambdaExpression<Sample>();
        });
    }

    [Fact]
    public void ShouldRenameParameterNames()
    {
        var conditions = new Condition
        {
            Group = new List<Condition>
            {
                new Condition
                {
                    Column = "column1",
                    Operator = Operator.Equals,
                    Value = "some value"
                },
                new Condition
                {
                    Column = "column2",
                    Operator = Operator.IsGreaterThan,
                    Value = 20
                },
                new Condition
                {
                    Group = new List<Condition>
                    {
                        new Condition
                        {
                            Column = "column1",
                            Operator = Operator.Equals,
                            Value = "some value"
                        },
                        new Condition
                        {
                            Column = "column2",
                            Operator = Operator.IsGreaterThan,
                            Value = 20
                        },
                        new Condition
                        {
                            Column = "column3",
                            Operator = Operator.IsNull
                        }
                    }
                }
            }
        };

        var expression = conditions.GetFilterQuery<Sample>(out var parameters);

        parameters.Count.ShouldBe(4);
        for (var i = 0; i < parameters.Count; i++)
        {
            parameters.ContainsKey(Condition.ParameterPrefix + (i + 1)).ShouldBeTrue();
        }
    }
}