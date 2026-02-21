using Microsoft.Dafny;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dafny2Js;

/// <summary>
/// Generates a logic-surface.json describing the full verified logic layer.
/// </summary>
public static class LogicSurfaceEmitter
{
  public static string Generate(Microsoft.Dafny.Program program, string appCoreModule)
  {
    var datatypes = TypeExtractor.ExtractDatatypes(program);
    var functions = TypeExtractor.ExtractFunctions(program, appCoreModule);
    var claims = ClaimsExtractor.ExtractClaims(program);

    // Find Model type
    string? modelType = null;
    foreach (var module in program.Modules())
    {
      foreach (var decl in module.TopLevelDecls)
      {
        if (decl.Name == "Model")
        {
          if (decl is TypeSynonymDecl syn)
            modelType = syn.Rhs.ToString();
          else if (decl is IndDatatypeDecl)
            modelType = decl.Name; // Model is a datatype itself
        }
      }
    }

    // Find Action datatype → extract constructors with fields
    var actionDt = datatypes.FirstOrDefault(dt => dt.Name == "Action");
    var actions = actionDt?.Constructors.Select(c => new ActionEntry(
      c.Name,
      c.Fields.Select(f => new FieldEntry(f.Name, f.Type.ToString())).ToList()
    )).ToList() ?? new List<ActionEntry>();

    // Find Inv predicate from claims
    var invPredicate = claims.Predicates.FirstOrDefault(p => p.Name == "Inv");
    InvariantEntry? invariant = invPredicate != null
      ? new InvariantEntry(invPredicate.Name, invPredicate.Conjuncts ?? new List<string>())
      : null;

    // Build datatypes list (excluding Action since it's listed separately)
    var dtEntries = datatypes
      .Where(dt => dt.Name != "Action")
      .Select(dt => new DatatypeEntry(
        dt.Name,
        dt.ModuleName,
        dt.Constructors.Select(c => new CtorEntry(
          c.Name,
          c.Fields.Select(f => new FieldEntry(f.Name, f.Type.ToString())).ToList()
        )).ToList()
      )).ToList();

    // Build AppCore functions
    var appCoreFns = functions.Select(f => new FunctionEntry(
      f.Name,
      f.Parameters.Select(p => new FieldEntry(p.Name, p.Type.ToString())).ToList(),
      f.ReturnType.ToString()
    )).ToList();

    var surface = new LogicSurface(
      modelType,
      actions,
      dtEntries,
      invariant,
      appCoreFns,
      functions.Count > 0,
      claims
    );

    var options = new JsonSerializerOptions
    {
      WriteIndented = true,
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    return JsonSerializer.Serialize(surface, options);
  }
}

// --- JSON shape records ---

record LogicSurface(
  string? Model,
  List<ActionEntry> Actions,
  List<DatatypeEntry> Datatypes,
  InvariantEntry? Invariant,
  List<FunctionEntry> AppCoreFunctions,
  bool HasAppCore,
  ClaimsResult Claims
);

record ActionEntry(string Name, List<FieldEntry> Fields);
record FieldEntry(string Name, string Type);
record DatatypeEntry(string Name, string Module, List<CtorEntry> Constructors);
record CtorEntry(string Name, List<FieldEntry> Fields);
record InvariantEntry(string Name, List<string> Conjuncts);
record FunctionEntry(string Name, List<FieldEntry> Params, string ReturnType);
