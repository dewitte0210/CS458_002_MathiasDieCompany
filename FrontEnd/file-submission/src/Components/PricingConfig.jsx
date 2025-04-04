import React from "react";
import styles from "./PricingConfig.module.css"


export default function PricingConfig(props) {
    
    
    return (
        <div className={styles["nav-bar"]}>
            <div className={styles["nav-items"]}>
                Feature Configuration
            </div>
            <div className={styles["nav-items"]}>
                Punch Configuration
            </div>
            <div className={styles["nav-items"]}>
                Rules configuration
            </div>
        </div>
    );
}