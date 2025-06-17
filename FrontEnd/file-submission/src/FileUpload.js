import React, { useState } from "react";

const uploadFile = async (file)  => {
    console.log(file);
    const formData = new FormData();
    formData.append("file", file);
    try {
        const res = await fetch("https://localhost:7244/api/FeatureRecognition/uploadFile", {
            method: "POST",
            body: formData
        });
        console.log(res);
    } catch (error) {
        console.error("Error uploading file:", error);
    }
};

// Export the uploadFile function.
export { uploadFile };

