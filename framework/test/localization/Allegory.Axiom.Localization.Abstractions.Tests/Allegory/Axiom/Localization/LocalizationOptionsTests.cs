using System;
using System.Globalization;
using System.Linq;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Localization;

public class LocalizationOptionsTests
{
    [Fact]
    public void ShouldAddResourceViaGenericExtension()
    {
        var options = new LocalizationOptions();

        options.Resources.Add<LocalizationOptionsTests>(
            defaultCulture: "en",
            paths: ["/i18n"]);

        options.Resources.ShouldHaveSingleItem();
        options.Resources.First().Name.ShouldBe(typeof(LocalizationOptionsTests).FullName);
        options.Resources.First().DefaultCulture.ShouldBe(new CultureInfo("en"));
        options.Resources.First().Paths.ShouldBe(["/i18n"]);
    }

    [Fact]
    public void ShouldAddResourceViaStringExtension()
    {
        var options = new LocalizationOptions();

        options.Resources.Add("MyApp.Resources.Messages",
            defaultCulture: "en",
            paths: ["/i18n/messages"]);

        options.Resources.ShouldHaveSingleItem();
        options.Resources.First().Name.ShouldBe("MyApp.Resources.Messages");
        options.Resources.First().DefaultCulture.ShouldBe(new CultureInfo("en"));
        options.Resources.First().Paths.ShouldBe(["/i18n/messages"]);
    }

    [Fact]
    public void ShouldThrowWhenAddingSameResourceTwice()
    {
        var options = new LocalizationOptions();
        options.Resources.Add<LocalizationOptionsTests>("en", ["/i18n"]);

        Should.Throw<ArgumentException>(() =>
            options.Resources.Add<LocalizationOptionsTests>("tr", ["/i18n2"]));

        Should.Throw<ArgumentException>(() =>
            options.Resources.Add(typeof(LocalizationOptionsTests).FullName!, "tr", paths: ["/i18n2"]));
    }

    [Fact]
    public void ShouldAddMultipleDistinctResources()
    {
        var options = new LocalizationOptions();
        options.Resources.Add("MyApp.Resources.Messages", "en", paths: ["/i18n/messages"]);
        options.Resources.Add("MyApp.Resources.Errors", "en", paths: ["/i18n/errors"]);

        options.Resources.Count.ShouldBe(2);
    }

    [Fact]
    public void ShouldGetResourceViaGenericExtension()
    {
        var options = new LocalizationOptions();

        options.Resources.Add<LocalizationOptionsTests>("en", paths: ["/i18n"]);

        var localizationResourceOptions = options.Resources.Get<LocalizationOptionsTests>();
        localizationResourceOptions.Name.ShouldBe(typeof(LocalizationOptionsTests).FullName);
        localizationResourceOptions.DefaultCulture.ShouldBe(new CultureInfo("en"));
        localizationResourceOptions.Paths.ShouldBe(["/i18n"]);
    }

    [Fact]
    public void ShouldGetResourceViaStringExtension()
    {
        var options = new LocalizationOptions();

        options.Resources.Add("MyApp.Resources.Messages", "en", paths: ["/i18n/messages"]);

        var localizationResourceOptions = options.Resources.Get("MyApp.Resources.Messages");
        localizationResourceOptions.Name.ShouldBe("MyApp.Resources.Messages");
        localizationResourceOptions.DefaultCulture.ShouldBe(new CultureInfo("en"));
        localizationResourceOptions.Paths.ShouldBe(["/i18n/messages"]);
    }

    [Fact]
    public void ShouldAddNewPathsToExistingResource()
    {
        var options = new LocalizationOptions();

        options.Resources.Add<LocalizationOptionsTests>("en", paths: ["/i18n"]);
        options.Resources.First().Paths.ShouldBe(["/i18n"]);

        options.Resources.Get<LocalizationOptionsTests>().AddPaths("/i18n2", "/i18n3");
        options.Resources.First().Paths.ShouldBe(["/i18n", "/i18n2", "/i18n3"]);
    }

    [Fact]
    public void ShouldMapExceptionCodeViaGenericExtension()
    {
        var options = new LocalizationOptions();
        options.Resources.Add<LocalizationOptionsTests>("en", paths: ["/i18n"]);

        options.MapExceptionCode<LocalizationOptionsTests>("ERR");

        options.ExceptionCodeMappings.ShouldHaveSingleItem();
        options.ExceptionCodeMappings["ERR"].ShouldBe(typeof(LocalizationOptionsTests).FullName);
    }

    [Fact]
    public void ShouldMapExceptionCodeViaStringExtension()
    {
        var options = new LocalizationOptions();

        options.MapExceptionCode("ERR", "MyApp.Resources.Messages");

        options.ExceptionCodeMappings.ShouldHaveSingleItem();
        options.ExceptionCodeMappings["ERR"].ShouldBe("MyApp.Resources.Messages");
    }

    [Fact]
    public void ShouldOverwriteExceptionCodeMappingForSamePrefix()
    {
        var options = new LocalizationOptions();

        options.MapExceptionCode("ERR", "MyApp.Resources.Messages");
        options.MapExceptionCode("ERR", "MyApp.Resources.Errors");

        options.ExceptionCodeMappings.ShouldHaveSingleItem();
        options.ExceptionCodeMappings["ERR"].ShouldBe("MyApp.Resources.Errors");
    }

    [Fact]
    public void ShouldMapMultipleDistinctExceptionCodes()
    {
        var options = new LocalizationOptions();

        options.MapExceptionCode("MSG", "MyApp.Resources.Messages");
        options.MapExceptionCode("ERR", "MyApp.Resources.Errors");

        options.ExceptionCodeMappings.Count.ShouldBe(2);
        options.ExceptionCodeMappings["MSG"].ShouldBe("MyApp.Resources.Messages");
        options.ExceptionCodeMappings["ERR"].ShouldBe("MyApp.Resources.Errors");
    }
}