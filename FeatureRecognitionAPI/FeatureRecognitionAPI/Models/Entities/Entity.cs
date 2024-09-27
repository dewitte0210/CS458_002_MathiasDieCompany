/*
 * Abstract class meant to be inherrited by every Entity child
 * The info from a PDF and DWG will be parsed into Entities
 *  - Line, Circle, Arc
 */
using System;
using System.IO;
using System.Numerics;

abstract public class Entity
{
    protected PossibleEntityTypes entityType;
    protected enum PossibleEntityTypes
    {
        line,
        circle,
        arc
    }
    
    public Entity()
    {

    }
}
