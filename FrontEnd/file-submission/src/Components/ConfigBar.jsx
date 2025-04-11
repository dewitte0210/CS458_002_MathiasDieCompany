import React from "react";
import styles from "./ConfigBar.module.css"


export default function ConfigBar({setActiveSection}) {
    const handleClickFeatures = (e) => {
        e.preventDefault();
        setActiveSection("features");
    }
    const handleClickPunches = (e) => {
        e.preventDefault();
        setActiveSection("punches");
    }
    const handleClickRules = (e) => {
        e.preventDefault();
        setActiveSection("rules");
    }
    return (
        <div className={styles.navBar}>
            <div className={styles.navItems}>
                <a className={styles.navText} href={'/config/features'} onClick={handleClickFeatures}>
                    Feature Configuration
                </a>
            </div>
            <div className={styles.navItems}>
                <a className={styles.navText} href={'/config/punches'} onClick={handleClickPunches}>
                    Punch Configuration
                </a>
            </div>
            <div className={styles.navItems}>
                <a className={styles.navText} href={'/config/rules'} onClick={handleClickRules}>
                    Rules configuration
                </a>
            </div>
        </div>
    );
}