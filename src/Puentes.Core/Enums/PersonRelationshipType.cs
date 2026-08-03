namespace Puentes.Shared.Enums;

public enum PersonRelationshipType
{
    Unknown = 0,
    Child = 1, // hijo o hija.
    Parent = 2, // padre o madre.
    Partner = 3, // pareja.
    Spouse = 4, // cónyuge.
    Sibling = 5, // hermano o hermana.
    Grandchild = 6, // nieto o nieta.
    Grandparent = 7, // abuelo o abuela.
    Friend = 8, // amigo.
    Caregiver = 9, // cuidador.
    Cohabitant = 10, // conviviente.
    NieceNephew = 11,    // sobrina o sobrino.
    AuntUncle= 12,    //tía o tío.
    DaughterSonInLaw= 13,    //nuera o yerno.
    ParentInLaw= 14,    //suegra o suegro.
    Cousin= 15,    //  prima o primo.
    Other = 99
}
