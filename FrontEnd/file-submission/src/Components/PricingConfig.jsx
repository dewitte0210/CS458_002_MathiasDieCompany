import React, {useState} from "react";
import ConfigBar from "./ConfigBar.jsx";
import FeatureConfig from "./FeatureConfig.jsx";
import PunchConfig from "./PunchConfig.jsx";
import RulesConfig from "./RulesConfig.jsx";

export default function PricingConfig(props) {
    const [activeSection, setActiveSection] = useState("features");
    
    return (
        <div>
            <ConfigBar setActiveSection={setActiveSection} />
            {activeSection === "features" && (<FeatureConfig />)}
            {activeSection === "punches" && (<PunchConfig />)}
            {activeSection === "rules" && (<RulesConfig />)}
        </div>
    );
}