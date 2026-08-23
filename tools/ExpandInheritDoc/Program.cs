using System.Xml.Linq;

namespace ExpandInheritDoc {
    /// <summary>
    /// Expands inheritdoc elements in a generated XML documentation file.
    /// </summary>
    internal class Program {
        // Returns an exit code for the shell.
        private static int Main(string[] args) {
            if (args.Length != 1) {
                Console.Error.WriteLine("Usage: ExpandInheritDoc <xml-documentation-file>");
                return 2;
            }

            string file = Path.GetFullPath(args[0].Replace('\\', Path.DirectorySeparatorChar));
            if (!File.Exists(file)) {
                Console.Error.WriteLine($"XML documentation file not found: {file}");
                return 1;
            }

            // Load the documentation and index each member by its name.
            XDocument document = XDocument.Load(file, LoadOptions.PreserveWhitespace);
            Dictionary<string, XElement> members = document
                .Descendants("member")
                .Where(member => member.Attribute("name") is not null)
                .ToDictionary(member => member.Attribute("name")!.Value);
            int skipped = 0;

            // Expand every member. A member can inherit documentation from another member.
            foreach (XElement member in members.Values) {
                if (!Expand(member, members, new HashSet<string>(StringComparer.Ordinal), ref skipped)) {
                    return 1;
                }
            }

            document.Save(file);
            Console.WriteLine($"Expanded inheritdoc comments in {file}; skipped {skipped} references without source documentation.");
            return 0;
        }

        // Replaces inheritdoc elements with documentation copied from their source member.
        private static bool Expand(
            XElement member,
            IReadOnlyDictionary<string, XElement> members,
            HashSet<string> resolving,
            ref int skipped) {
            string memberName = member.Attribute("name")!.Value;
            if (!resolving.Add(memberName)) {
                Console.Error.WriteLine($"Circular inheritdoc reference found for '{memberName}'.");
                return false;
            }

            foreach (XElement inheritdoc in member.Descendants("inheritdoc").ToArray()) {
                string? cref = inheritdoc.Attribute("cref")?.Value;
                if (string.IsNullOrEmpty(cref) || !members.TryGetValue(cref, out XElement? source)) {
                    skipped++;
                    inheritdoc.Remove();
                    continue;
                }

                // Resolve the source first because it may contain its own inheritdoc element.
                if (!Expand(source, members, resolving, ref skipped)) {
                    return false;
                }

                // Copy source elements that are not already present on the target member.
                foreach (XElement element in source.Elements()) {
                    if (!HasEquivalentElement(inheritdoc.Parent!, element)) {
                        inheritdoc.AddBeforeSelf(new XElement(element));
                    }
                }

                inheritdoc.Remove();
            }

            resolving.Remove(memberName);
            return true;
        }

        // Checks if the target already has the same documentation element.
        private static bool HasEquivalentElement(XElement parent, XElement candidate) {
            XAttribute? candidateKey = candidate.Attribute("name") ?? candidate.Attribute("cref");
            return parent.Elements(candidate.Name).Any(existing => {
                XAttribute? existingKey = existing.Attribute("name") ?? existing.Attribute("cref");
                return candidateKey is null ? existingKey is null : existingKey?.Value == candidateKey.Value;
            });
        }
    }
}
