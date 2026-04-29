using Godot;

namespace Maze;

/// <summary>
/// Wurzelskript der Hauptszene. Verbindet HUD, Datenmodell und die aktive View.
/// In dieser Phase ist Main noch ein leeres Skelett mit allen Lebenszyklus-Methoden.
/// </summary>
public partial class Main : Node
{
    // Wird aufgerufen, wenn der Knoten zum SceneTree hinzugefuegt wurde
    // und alle Kinder ebenfalls bereit sind. Hier werden spaeter Referenzen
    // auf HUD und Views aufgeloest.
    public override void _Ready()
    {
        GD.Print("[Main] _Ready: Hauptszene wurde geladen.");
    }

    // Wird in jedem Frame aufgerufen. Wir nutzen es vorerst nicht aktiv,
    // implementieren es aber, damit Schueler die Standard-Lifecycle-Hooks sehen.
    public override void _Process(double delta)
    {
        // Bewusst leer. Spaetere Phasen reichen das delta an den AlgorithmRunner.
    }

    // Wird mit fester Frequenz aufgerufen (Standard 60 Hz, fuer Physik).
    // Fuer unser Projekt nicht zwingend notwendig, der Vollstaendigkeit halber.
    public override void _PhysicsProcess(double delta)
    {
        // Bewusst leer.
    }

    // Letzter Hook vor dem Entfernen aus dem SceneTree. Hier werden spaeter
    // laufende Coroutinen / Timer / Aufgaben sauber beendet.
    public override void _ExitTree()
    {
        GD.Print("[Main] _ExitTree: Hauptszene wird verlassen.");
    }
}