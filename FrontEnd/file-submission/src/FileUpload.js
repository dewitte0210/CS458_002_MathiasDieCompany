import React, { useState } from "react";
//import axios from "axios";

// export const FileUpload = () => {
//     const [file, setFile] = useState();

//     const saveFile = (e) => {
//         console.log(e.target.files[0]);
//         setFile(e.target.files[0]);
//         setFileName(e.target.files[0].name);
//     };

//     const uploadFile = async (e) => {
//         console.log(file);
//         const formData = new FormData();
//         formData.append("file", file);
//         try {
//             const res = await axios.post("https:localhost:5283/api/FeatureRecognition/uploadFile");
//         }
//     };

//     return (
//         <div>
//             <input type="file" onChange={saveFile} />
//             <button onClick={uploadFile}>Upload</button>
//         </div>
//     );
// };

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

// Export the uploadFile function
export { uploadFile };

