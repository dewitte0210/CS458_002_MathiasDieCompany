import * as React from 'react';
import { useState } from 'react';
import DragDropZone from './DragDropZone';
import JsonTable from './JsonTable';
import LoadingIndicator from './LoadingIndicator';

/*
  Defines the shape of the props that the UploadAndShow component accepts.
*/
interface UploadAndShowProps {
  onFilesSelected?: (files: File[]) => void;
}

/*
  Main component that handles drag-and-drop and file submission.
*/
const UploadAndShow: React.FC<UploadAndShowProps> = ({ onFilesSelected }) => {
  const [file, setFile] = useState<File | null>(null);
  const [submitted, setSubmitted] = useState(false);
  const [jsonResponse, setJsonResponse] = useState<any>(null);
  const [isLoading, setIsLoading] = useState(false);

  const allowedFileExtensions = ['.pdf', '.dwg', '.dxf'];

  const handleSubmit = async () => {
    if (!file) return;

    setIsLoading(true);

    const formData = new FormData();
    formData.append("file", file);

    try {
      const res = await fetch("https://localhost:44373/api/FeatureRecognition/uploadFile", {
        method: "POST",
        body: formData
      });

      if (!res.ok) throw new Error(`Server error: ${res.status} ${res.statusText}`);

      const jsonResponse = await res.json();
      setJsonResponse(jsonResponse);
      setSubmitted(true);

    } catch (error) {
      alert('An error occurred while submitting the file. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const backToUpload = () => {
    setSubmitted(false);
    setFile(null);
    setJsonResponse(null);
  };

  return (
    <section className="drag-drop">
      {isLoading ? (
        <LoadingIndicator />
      ) : !submitted ? (
        <DragDropZone
          file={file}
          allowedFileExtensions={allowedFileExtensions}
          setFile={setFile}
          onFilesSelected={onFilesSelected}
          onSubmit={handleSubmit}
        />
      ) : (
        <div className="success-message">
          {jsonResponse && (
            <JsonTable jsonResponse={jsonResponse} />
          )}
          <button className="animated-button" onClick={backToUpload}>
            Go Back
          </button>
        </div>
      )}
    </section>
  );
};

export default UploadAndShow;
