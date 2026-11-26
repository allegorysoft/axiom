namespace Allegory.Axiom.Security.Encryption;

public class StringEncryptionOptions
{
    public const string Section = "Axiom:Security:Encryption";

    public string PassPhrase { get; set; } = "Allegory.Axiom.Default";

    public int Iterations { get; set; } = 100_000;
}