using Puentes.Core.Domain;

namespace Puentes.Core.Requests;
/// <summary>
/// Request to register an event for a person
/// es inmutable a diferencia de la clase Event que es mutable.
/// Para modificar un record se hace:
/*var nuevo = person with
{
    Name = "Juan"
};
*/
///Crea un nuevo objeto con los valores modificados, dejando el original intacto.
/// </summary>
/// <param name="PersonId"></param>
/// <param name="Type"></param>
/// <param name="Description"></param>
public record RegisterEventRequest(
Guid PersonId,
EventType Type,
string Description
);
