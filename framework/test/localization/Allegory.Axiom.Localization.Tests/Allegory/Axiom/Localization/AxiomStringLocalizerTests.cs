using System.Globalization;
using System.Linq;
using Allegory.Axiom.Localization.Resources;
using Microsoft.Extensions.Localization;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Localization;

public class AxiomStringLocalizerTests : HostedIntegrationTestBase
{
    protected IStringLocalizer<AxiomLocalizationResource> Localizer => Service<IStringLocalizer<AxiomLocalizationResource>>();

    [Fact]
    public void ShouldReturnTranslationForCurrentCulture()
    {
        CultureInfo.CurrentUICulture = new CultureInfo("en");
        var result1 = Localizer["txt1"];
        result1.ResourceNotFound.ShouldBeFalse();
        result1.Value.ShouldBe("txt1 - en.json");

        CultureInfo.CurrentUICulture = new CultureInfo("en-US");
        var result2 = Localizer["txt1"];
        result2.ResourceNotFound.ShouldBeFalse();
        result2.Value.ShouldBe("txt1 - en-US.json");

        CultureInfo.CurrentUICulture = new CultureInfo("tr");
        var result3 = Localizer["txt1"];
        result3.ResourceNotFound.ShouldBeFalse();
        result3.Value.ShouldBe("txt1 - tr.json");
    }

    [Fact]
    public void ShouldReturnFormattedTranslationForCurrentCulture()
    {
        CultureInfo.CurrentUICulture = new CultureInfo("en");

        var result = Localizer["txt-formatted", "extra"];

        result.Value.ShouldBe("extra, formatted");
    }

    [Fact]
    public void ShouldFallbackToParentCultureWhenSpecificCultureKeyMissing()
    {
        CultureInfo.CurrentUICulture = new CultureInfo("tr-TR");

        // txt1 exists in tr.json, not tr-TR.json
        var result = Localizer["txt1"];

        result.ResourceNotFound.ShouldBeFalse();
        result.Value.ShouldBe("txt1 - tr.json");
    }

    [Fact]
    public void ShouldFallbackToDefaultCultureWhenSpecificCultureKeyMissing()
    {
        CultureInfo.CurrentUICulture = new CultureInfo("tr");

        // txt2 exists in en.json (Default), not tr.json
        var result = Localizer["txt2"];

        result.ResourceNotFound.ShouldBeFalse();
        result.Value.ShouldBe("txt2 - en.json");
    }

    [Fact]
    public void ShouldFallbackToDefaultCultureWhenSpecifiedCultureMissing()
    {
        CultureInfo.CurrentUICulture = new CultureInfo("fr");

        // fr culture not exists, fallback to default "en"
        var result = Localizer["txt1"];

        result.ResourceNotFound.ShouldBeFalse();
        result.Value.ShouldBe("txt1 - en.json");
    }

    [Fact]
    public void ShouldReturnKeyAsValueWhenTranslationNotFound()
    {
        CultureInfo.CurrentUICulture = new CultureInfo("en");

        var result = Localizer["nonexistent_key"];

        result.ResourceNotFound.ShouldBeTrue();
        result.Value.ShouldBe("nonexistent_key");
    }

    [Fact]
    public void ShouldMergeTranslationsFromMultipleDirectories()
    {
        CultureInfo.CurrentUICulture = new CultureInfo("tr");

        // txt3 only in Directory-2/tr.json
        var result = Localizer["txt3"];

        result.ResourceNotFound.ShouldBeFalse();
        result.Value.ShouldBe("txt3 - tr.json (Directory-2)");
    }

    [Fact]
    public void ShouldGetAllStringsWithoutParents()
    {
        CultureInfo.CurrentUICulture = new CultureInfo("en-US");

        var strings = Localizer.GetAllStrings(includeParentCultures: false);

        // en-US.json only has txt1
        strings.ShouldHaveSingleItem();
    }

    [Fact]
    public void ShouldGetAllStringsWithParentCultures()
    {
        CultureInfo.CurrentUICulture = new CultureInfo("tr-TR");

        var strings = Localizer.GetAllStrings(includeParentCultures: true).ToList();

        //tr-TR.json        : hasn't any translation
        //tr.json           : txt1, txt3(Directory-2)
        //en.json (Default) : txt1, txt2, txt-formatted
        //merged unique     : txt1 (tr), txt3 (tr), txt2 (en), txt-formatted (en)
        strings.ShouldContain(s => s.Name == "txt1" && s.Value == "txt1 - tr.json");
        strings.ShouldContain(s => s.Name == "txt3" && s.Value == "txt3 - tr.json (Directory-2)");
        strings.ShouldContain(s => s.Name == "txt2" && s.Value == "txt2 - en.json");
        strings.ShouldContain(s => s.Name == "txt-formatted" && s.Value == "{0}, formatted");
    }

    [Fact]
    public void ShouldNotDuplicateKeyWhenParentAndChildBothHaveIt()
    {
        CultureInfo.CurrentUICulture = new CultureInfo("en-US");

        var strings = Localizer.GetAllStrings(includeParentCultures: true);

        strings.Count(f => f.Name == "txt1").ShouldBe(1);
    }

    [Fact]
    public void ShouldReflectDynamicTranslationChangesAcrossAllLocalizers()
    {
        CultureInfo.CurrentUICulture = new CultureInfo("en");
        var factory = Service<IStringLocalizerFactory>();
        var localizer1 = (IAxiomStringLocalizer) factory.Create(typeof(AxiomLocalizationResource));
        var localizer2 = factory.Create(typeof(AxiomLocalizationResource).FullName!, string.Empty);
        var localizer3 = Service<IStringLocalizer<AxiomLocalizationResource>>();

        localizer1.Translations["en"].TryGetValue("some-key", out _).ShouldBeFalse();
        localizer2["some-key"].ResourceNotFound.ShouldBeTrue();
        localizer3["some-key"].ResourceNotFound.ShouldBeTrue();

        localizer1.Translations["en"]["some-key"] = "dynamic-translation";

        localizer1.Translations["en"].TryGetValue("some-key", out _).ShouldBeTrue();
        localizer2["some-key"].ResourceNotFound.ShouldBeFalse();
        localizer2["some-key"].Value.ShouldBe("dynamic-translation");
        localizer3["some-key"].ResourceNotFound.ShouldBeFalse();
        localizer3["some-key"].Value.ShouldBe("dynamic-translation");
    }
}