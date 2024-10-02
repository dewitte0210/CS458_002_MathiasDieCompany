/*
 * Abstract class meant to be inherrited by every Entity child
 * The info from a PDF and DWG will be parsed into Entities
 *  - Line, Circle, Arc
 */
using System;
using System.IO;
using System.Numerics;

namespace FeatureRecognitionAPI.Models
{
    protected PossibleEntityTypes entityType;
    double length;
    protected enum PossibleEntityTypes
    {
        line,
        circle,
        arc
    }
    
    public void setLength(double length)
    {
        this.length = length;
    }
    public double getLength() { return this.length; }
    public Entity()
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
}
