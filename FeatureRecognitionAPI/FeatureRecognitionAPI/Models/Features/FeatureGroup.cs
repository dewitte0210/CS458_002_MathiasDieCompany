using FeatureRecognitionAPI.Services;
using System.Runtime.Intrinsics.Arm;

namespace FeatureRecognitionAPI.Models.Features
{
    public class FeatureGroup
    {
        public int Count { get; set; } //Track how many feature groups of this type are found
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

                if (this.totalArcs == ((FeatureGroup)obj).totalArcs
                    && this.totalLines == ((FeatureGroup)obj).totalLines
                    && this.totalCircles == ((FeatureGroup)obj).totalCircles
                    )
                {
                    //Sort by perimiter
                    features.Sort((x,y) => x.perimeter.CompareTo(y.perimeter));
                    ((FeatureGroup)obj).features.Sort ((x,y) => x.perimeter.CompareTo((y.perimeter)));


                    for (int i = 0; i < features.Count; i++)
                    {
                        //While this features @ i has same permiter as obj.features[j] check if any j = features[i]
                        int j = i;
                        bool checkPoint = false;
                        while (j < features.Count 
                            && Math.Abs( features[i].perimeter - ((FeatureGroup)obj).features[j].perimeter) < Entity.EntityTolerance) 
                        {
                            if (features[i].Equals(features[j]))
                            {
                                checkPoint = true;
                                break;
                            }
                            //If first element checked isn't equal, increment J
                            j++;
                        }
                        if (!checkPoint)
                        {
                            return false;
                        }
                    }
                    //If we got here, checkPoint was never false, so return true;
                    return true;
                }
                else
                {
                    return false;
                }

            }

        }
    }
}