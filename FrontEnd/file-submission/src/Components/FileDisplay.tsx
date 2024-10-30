import * as React from 'react';
import { MdClear } from "react-icons/md";

/*
  Displays selected file with option to remove.
*/
interface FileDisplayProps {
  file: File;
  onRemoveFile: () => void;
}

const FileDisplay: React.FC<FileDisplayProps> = ({ file, onRemoveFile }) => (
  <div className="file-list">
    <div className="file-list__container">
      <div className="file-item">
        <div className="file-info">
          <p>{file.name}</p>
        </div>
        <div className="file-actions">
          <MdClear onClick={onRemoveFile} />
        </div>
      </div>
    </div>
  </div>
);

export default FileDisplay;
