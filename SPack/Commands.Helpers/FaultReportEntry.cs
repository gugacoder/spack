using ScriptPack.Domain;

namespace SPack.Commands.Helpers;

/// <summary>
/// Representação de uma entrada de falhas no relatórios de falhas.
/// </summary>
/// <param name="Node">
/// Nodo que contém as falhas.
/// </param>
/// <param name="Faults">
/// Lista de falhas ocorridas no nodo.
/// </param>
public record FaultReportEntry(INode Node, Fault[] Faults);
