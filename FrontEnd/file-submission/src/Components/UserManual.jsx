import React, { useState, useRef } from "react";
import styles from "./UserManual.module.css";
import "@fortawesome/fontawesome-free/css/all.min.css"

import Introduction from "./UserManualContents/Introduction";
import DetailedInstructions from "./UserManualContents/DetailedInstructions";
import FAQs from "./UserManualContents/FAQs";
import Contact from "./UserManualContents/Contact";

/**
 * UserManual - Handles a user manual with key information on using the app
 * 
 * Usage:
 * <UserManual/>
 */
export default function UserManual() {

    const [isManualVisible, setIsManualVisible] = useState(false); //Saves panel state during redraws
    const [isManualHidden, setIsManualHidden] = useState(true);

    //State to keep track of the active manual section
    const [activeSection, setActiveSection] = useState("introduction");
    const manualButtonRef = useRef(null); // Reference to the manual button


    // Function to set manual panel position dynamically based on the button position
    const toggleManual = () => {
        if (!isManualVisible && manualButtonRef.current) {
            const buttonRect = manualButtonRef.current.getBoundingClientRect();
        }
        setIsManualVisible(!isManualVisible);
    };

    return (
        <div className={styles["fade-out-panel-container"]}>
            {/* Caret Button */}
            <button
                className={`${styles["manual-toggle-button"]} ${isManualHidden ? styles["button-slide-out"] : styles["button-slide-in"]}`}
                onClick={() => setIsManualHidden(prev => !prev)}
            >
                {isManualHidden ? < i className='fa-solid fa-question'></i> : <i className='fa-solid fa-caret-right'></i>}
            </button>

            {/* Manual Panel */}
            <div
                className={`
                ${styles["manual-dialog-modern"]}
                ${isManualHidden
                        ? styles["manual-slide-out"]
                        : styles["manual-slide-in"]
                    }
              `}
            >

                <div className={styles["manual-sidebar"]}>
                    <h3>User Manual</h3>
                    <ul>
                        <li onClick={() => setActiveSection("introduction")}>Introduction</li>
                        <li onClick={() => setActiveSection("instructions")}>Detailed Instructions</li>
                        <li onClick={() => setActiveSection("faqs")}>FAQs</li>
                        <li onClick={() => setActiveSection("contact")}>Contact</li>
                    </ul>
                </div>

                {/* Content inside manual */}
                <div className={styles["manual-content"]}>
                    {activeSection === "introduction" && <Introduction />}
                    {activeSection === "instructions" && <DetailedInstructions />}
                    {activeSection === "faqs" && <FAQs />}
                    {activeSection === "contact" && <Contact />}
                </div>
            </div>
        </div>
    );
}