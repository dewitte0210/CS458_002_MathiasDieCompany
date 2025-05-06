import React from "react";

const DetailedInstructions = () => (
    <div>
        <h2>Detailed Instructions</h2>
        <h4>1. Uploading a File</h4>
        <p>
            To upload a file, either click the upload button or drag the file directly
            onto it. Before uploading, ensure the file contains only supported features. Uploading
            a file with unsupported features may result in unpredictable behavior and could redirect
            you back to this page.
        </p>

        <h4>2. Viewing Results</h4>
        <p>
            If the upload is successful, you will be automatically redirected to the results screen.
            Here, you can review the detected features to ensure they accurately reflect the contents
            of your file. A visual representation of the file will be provided for easy reference, and
            any undetected features will be highlighted in red.
        </p>

        <h4>3. Editing the Results</h4>
        <p>
            If a feature was incorrectly detected or not recognized, you can manually add or remove
            features in the table to correct the issue. For kiss-cut features, check the corresponding
            box to ensure accurate pricing. If the detected quantity of identical dies is incorrect,
            you can adjust the number by entering the correct value directly or using the arrows provided
            in the input field at the top of the table.
        </p>

        <h4>4. Selecting Special Fields</h4>
        <p>
            To receive a price estimate, the <b>Rule Type</b>, <b>Ejection Method</b>, and any
            applicable <b>Punch Types</b> must be specified. You can complete these fields by selecting
            the appropriate options from each corresponding dropdown menu.
        </p>

        <h4>5. The Price Estimate</h4>
        <p>
            Once all selections are complete and accurate, click the <b>Confirm</b> button below the table.
            The resulting screen will display a price estimate for producing the die cut. If you need to make
            further adjustments, you can return to the previous screen using the <b>Back to Feature List</b> button.
            You may also return to the file upload screen from this page, but please note that your current die
            cut configuration will not be saved. When you're ready to request production, click
            the <b>Confirm</b> button again.
        </p>
        <p>
            <b>Note: The estimated price may differ from the actual cost of the die cut. If you have any
                questions, please refer to the Contact section of this help guide and send your inquiries
                to the listed email address.</b>
        </p>
    </div>
);

export default DetailedInstructions;
