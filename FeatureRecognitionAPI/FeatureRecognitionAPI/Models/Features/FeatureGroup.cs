using FeatureRecognitionAPI.Services;
using System.Runtime.Intrinsics.Arm;

namespace FeatureRecognitionAPI.Models.Features
{
    public class FeatureGroup
    {
        protected int count; //Track how many feature groups of this type are found
        protected int totalArcs;
        protected int totalLines;
        protected int totalCircles;
        protected List<Feature> features;
        public FeatureGroup( List<Feature> features) 
        {
           // this.count = count;
            this.features = features;

            foreach( Feature feature in features)
            {
                this.totalArcs += feature.getNumArcs();
                this.totalLines += feature.getNumLines();
                this.totalCircles += feature.getNumCircles();
            }

        }

        //Check of all features in the group have a corresponding feature in other group, return true if they do
        public override bool Equals(object? obj)
        {
            if (!(obj is FeatureGroup) || obj == null)
            {
                return false;
            }
            else if (obj == this)
            {
                return true;
            }
            else
            {
                //If in here, obj is a FeatureGroup

                if (totalArcs == ((FeatureGroup)obj).totalArcs
                    && totalLines == ((FeatureGroup)obj).totalLines
                    && totalCircles == ((FeatureGroup)obj).totalCircles)
                {
                    //Do something
                }
                else
                {
                    return false;
                }

            }

        }
    }
}