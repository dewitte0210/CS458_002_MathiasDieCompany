import React from "react";

const FAQs = () => (
    <div>
        <h2>FAQs</h2>
        <p>
            <b>What file types are supported?</b>
        </p>
        <p>
            Currently, only DXF and DWG file formats are supported. Support for additional
            file types will be added in the future.
        </p>
        <br></br>

        <p>
            <b>What if one of the features in my file is not in the supported features list?</b>
        </p>
        <p>
            Submitting a file with unsupported features may result in unexpected behavior or redirect
            you back to the initial page. Expanded feature detection support will be available in future updates.
        </p>
        <br></br>

        <p>
            <b>Some features in my file weren't detected or were misclassified - what can I do?</b>
        </p>
        <p>
            You can add or remove features in the table to correct any missed detections or misclassifications.
        </p>
        <br></br>

        <p>
            <b>What does it mean to mark a feature as a kiss cut?</b>
        </p>
        <p>
            A kiss-cut feature is a cut that leaves some of the material intact during the cutting process.
            Any feature with a checkmark in the corresponding box will be treated as a kiss-cut.
        </p>
        <br></br>

        <p>
            <b>Why does my price estimate seem off?</b>
        </p>
        <p>
            The price estimate may differ from the actual cost of the die cut. Please ensure that all
            detected features accurately reflect those in your file, and make any necessary adjustments.
            If the estimate still appears incorrect after reviewing and editing the features, refer to the
            Contact section of this help guide to submit an inquiry.
        </p>
        <br></br>

        <p>
            <b>Can I go back and make changes after getting a price estimate?</b>
        </p>
        <p>
            Yes, you can click the <b>Back to Feature List</b> button to return and make any necessary changes.
            Please note that clicking the <b>Back to File Upload</b> button will discard your current die configuration.
        </p>
        <br></br>

        <p>
            <b>Will my configuration be saved if I leave the page?</b>
        </p>
        <p>
            No, the die configuration will not be saved if you leave the page for any reason.
        </p>
        <br></br>

        <p>
            <b>Who should I contact if I have questions about pricing or any issues?</b>
        </p>
        <p>
            If you have any questions, please refer to the Contact section of this help guide and
            send your inquiries to the listed email address.
        </p>
    </div>
);

export default FAQs;
