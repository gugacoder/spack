using System.Diagnostics;
using System.Text;
using SPack.Library;

namespace SPack.Domain;

public class Fault : INode
{
  public static class Hints
  {
    public const string CyclicDependency = nameof(CyclicDependency);
    public const string NoConnectionSpecified = nameof(NoConnectionSpecified);
  }

  public Fault()
  {
  }

  public Fault(string message)
  {
    this.Message = message;
  }

  public INode? Parent { get; set; }

  public string Hint { get; set; } = string.Empty;

  public string Message { get; set; } = string.Empty;

  public string? Details { get; set; }

  public IEnumerable<INode> GetChildren()
  {
    yield break;
  }

  public void Accept(IVisitor visitor)
  {
    visitor.Visit(this);
  }

  public async Task AcceptAsync(IAsyncVisitor visitor)
  {
    await visitor.VisitAsync(this);
  }

  public static Fault FromException(Exception exception)
  {
    return new Fault
    {
      Hint = nameof(Exception),
      Message = exception.GetStackMessage(),
      Details = exception.GetStackTrace()
    };
  }

  public override string ToString() => $"{base.ToString()} {Message}";
}
