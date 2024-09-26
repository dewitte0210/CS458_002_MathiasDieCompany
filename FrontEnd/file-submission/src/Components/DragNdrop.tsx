import * as React from 'react';
import { AiOutlineCloudUpload } from "react-icons/ai";
import { MdClear } from "react-icons/md";
import "./drag-drop.css";
import { useState } from 'react';

interface DragNdropProps {
  onFilesSelected?: (files: File[]) => void;
  width: number;
  height: number;
}

const DragNdrop: React.FC<DragNdropProps> = ({
  onFilesSelected,
  width,
  height,
}) => {
  const [files, setFiles] = useState<File[]>([]);

  const handleDrop = (event: React.DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    const droppedFiles = event.dataTransfer.files;
    if (droppedFiles.length > 0) {
      const newFiles = Array.from(droppedFiles);
      setFiles((prevFiles) => [...prevFiles, ...newFiles]);
      if (onFilesSelected) {
        onFilesSelected(newFiles);
      }
    }
  };

  const handleRemoveFile = (index: number) => {
    setFiles((prevFiles) => prevFiles.filter((_, i) => i !== index));
  };

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFiles = event.target.files;
    if (selectedFiles && selectedFiles.length > 0) {
      const newFiles = Array.from(selectedFiles);
      setFiles((prevFiles) => [...prevFiles, ...newFiles]);
    }
  };
  

  return (
    <section className="drag-drop" style={{ width: width, height: height }}>
      <div
        className={`document-uploader ${
          files.length > 0 ? "upload-box active" : "upload-box"
        }`}
        onDrop={handleDrop}
        onDragOver={(event) => event.preventDefault()}
      >
        <div className="upload-info">
          <AiOutlineCloudUpload />
          <div>
            <p>Drag and drop your files here</p>
            <p>
              Supported files: .DXF, .DWG, .PDF
            </p>
          </div>
        </div>
        <input
            type="file"
            hidden
            id="browse"
            onChange={handleFileChange}
            accept=".png, .jpg, .jpeg, .pdf,.docx,.pptx,.txt,.xlsx"
            multiple
          />
          <label htmlFor="browse" className="browse-btn">
            Browse files
          </label>
        {files.length > 0 && (
          <div className="file-list">
            <div className="file-list__container">
              {files.map((file, index) => (
                <div className="file-item" key={index}>
                  <div className="file-info">
                    <p>{file.name}</p>
                    {/* <p>{file.type}</p> */}
                  </div>
                  <div className="file-actions">
                    <MdClear onClick={() => handleRemoveFile(index)} />
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </section>
  );
};

export default DragNdrop;