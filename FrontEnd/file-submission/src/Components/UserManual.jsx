import React, { useState, useRef, useEffect, useLayoutEffect } from "react";
import styles from "./UserManual.module.css";
import "@fortawesome/fontawesome-free/css/all.min.css"

import Introduction from "./UserManualContents/Introduction";
import GettingStarted from "./UserManualContents/GettingStarted";
import Features from "./UserManualContents/Features";
import DetailedInstructions from "./UserManualContents/DetailedInstructions";
import Troubleshooting from "./UserManualContents/Troubleshooting";
import FAQs from "./UserManualContents/FAQs";
import Contact from "./UserManualContents/Contact";

/**
 * UserManual - Handles a user manual with key information on using the app
 * 
 * Usage:
 * <UserManual/>
 */
export default function UserManual (){
  
  const [isManualVisible, setIsManualVisible] = useState(false); //Saves panel state during redraws
  const [isManualHidden, setIsManualHidden] = useState(true);
  //const [panelPosition, setPanelPosition] = useState({ x: 100, y: 100 }); // Initial panel position
  const [dragging, setDragging] = useState(false); // State for dragging
  const [offset, setOffset] = useState({ x: 0, y: 0 }); // Offset to handle smooth dragging
  
  //State to keep track of the active manual section
  const [activeSection, setActiveSection] = useState("introduction");

  const manualButtonRef = useRef(null); // Reference to the manual button

  const [hasMounted, setHasMounted] = useState(false);

  useLayoutEffect(() => {
      requestAnimationFrame(() => {
          setHasMounted(true);
      });
  }, []);



  // Function to set manual panel position dynamically based on the button position
  const toggleManual = () => {
    if (!isManualVisible && manualButtonRef.current) {
      const buttonRect = manualButtonRef.current.getBoundingClientRect();
      //setPanelPosition({
      //  x: buttonRect.left - 500,
      //  y: buttonRect.top - 600,
      //});
    }
    setIsManualVisible(!isManualVisible);
  };

  // Function to handle drag move
  const handleMouseDown = (e) => {
    // Only start dragging if clicked within the panel (e.g., on header)
    setDragging(true);
    setOffset({
      x: e.clientX,
      y: e.clientY,
    });
  };

// Function to handle drag move
  //const handleMouseMove = (e) => {
  //  if (dragging) {
  //    const deltaX = e.clientX - offset.x;
  //    const deltaY = e.clientY - offset.y;

  //    const panelWidth = 300; // Adjust as needed
  //    const panelHeight = 400; // Adjust as needed

  //    const boundedX = Math.max(
  //        0,
  //        Math.min(window.innerWidth - panelWidth, panelPosition.x + deltaX)
  //    );
  //    const boundedY = Math.max(
  //        0,
  //        Math.min(window.innerHeight - panelHeight, panelPosition.y + deltaY)
  //    );

  //    setPanelPosition({
  //      x: boundedX,
  //      y: boundedY,
  //    });

  //    setOffset({
  //      x: e.clientX,
  //      y: e.clientY,
  //    });
  //  }
  //};

  // Handle Drag End
  //const handleMouseUp = () => setDragging(false);

  // Attach event listeners for dragging
  //React.useEffect(() => {
  //  if (dragging) {
  //    window.addEventListener("mousemove", handleMouseMove);
  //    window.addEventListener("mouseup", handleMouseUp);
  //  } else {
  //    window.removeEventListener("mousemove", handleMouseMove);
  //    window.removeEventListener("mouseup", handleMouseUp);
  //  }
  //  return () => {
  //    window.removeEventListener("mousemove", handleMouseMove);
  //    window.removeEventListener("mouseup", handleMouseUp);
  //  };
  //});

  return (
      <div className={styles["fade-out-panel-container"]}>
        {/* Manual book icon */}
        {/*<button className={styles["manual-button"]} ref={manualButtonRef} onClick={toggleManual}>*/}
        {/*  <i className='fas fa-question-circle'></i>*/}
        {/*</button>*/}

          {/* Caret Button */}
          <button
              className={`${styles["manual-toggle-button"]} ${isManualHidden ? styles["button-slide-out"] : styles["button-slide-in"]}`}
              onClick={() => setIsManualHidden(prev => !prev)}
          >
              {isManualHidden ? < i className='fa-solid fa-question'></i> : <i className='fa-solid fa-caret-right'></i>}
          </button>

        {/* Manual Panel */}
          {/*{isManualVisible && (*/}
              
             
            
          <div
              className={`
                ${styles["manual-dialog-modern"]}
                ${hasMounted
                      ? isManualHidden
                          ? styles["manual-slide-out"]
                          : styles["manual-slide-in"]
                      : styles["manual-hidden-initial"]
                  }
              `}
          >

              {/*Close*/}
              {/*<button className={styles["close-button"]} onClick={toggleManual}>*/}
              {/*  &times;*/}
              {/*    </button>*/}
                  

              <div className={styles["manual-sidebar"]}>
                <h3>User Manual</h3>
                <ul>
                  <li onClick={() => setActiveSection("introduction")}>Introduction</li>
                  {/*<li onClick={() => setActiveSection("getting-started")}>Getting Started</li>*/}
                  {/*<li onClick={() => setActiveSection("features")}>Features</li>*/}
                  <li onClick={() => setActiveSection("instructions")}>Detailed Instructions</li>
                  {/*<li onClick={() => setActiveSection("troubleshooting")}>Troubleshooting</li>*/}
                  <li onClick={() => setActiveSection("faqs")}>FAQs</li>
                  <li onClick={() => setActiveSection("contact")}>Contact</li>
                </ul>
               </div>
              
              {/* Content inside manual */}
              {/* TODO: For better maintainability, possibly change this to be read from file */}

                  <div className={styles["manual-content"]}>
                      {activeSection === "introduction" && <Introduction />}
                      {/*{activeSection === "getting-started" && <GettingStarted />}*/}
                      {/*{activeSection === "features" && <Features />}*/}
                      {activeSection === "instructions" && <DetailedInstructions />}
                      {/*{activeSection === "troubleshooting" && <Troubleshooting />}*/}
                      {activeSection === "faqs" && <FAQs />}
                      {activeSection === "contact" && <Contact />}
                  </div>

                      </div>
              
             
        {/*)}*/}
      </div>
  );
}