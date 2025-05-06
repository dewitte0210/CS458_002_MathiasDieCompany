import React from "react";

const Introduction = () => (
    <div>
        <h2>Introduction</h2>
        <p>Welcome to the Mathias Die Company - Price Estimator!</p>
        <p>
            This estimator allows you to upload a DXF or DWG file and automatically
            detects any supported features to generate a price estimate. Features
            are categorized based on their shape and associated cost, and are
            displayed alongside a visual representation of the die cut. If any
            features are missing or incorrectly categorized, you can manually edit
            them to ensure the die cut matches your exact specifications.
        </p>
        <p>
            <b>Note: The estimated price may differ from the actual cost of the die cut. If you have any
                questions, please refer to the Contact section of this help guide and send your inquiries
                to the listed email address.</b>
        </p>
    </div>
);

export default Introduction;
