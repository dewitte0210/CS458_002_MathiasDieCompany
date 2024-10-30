import * as React from 'react';
import UploadAndShow from './Components/UploadAndShow';
import "./Components/UploadAndShow.css";
import { useState } from 'react';

const App: React.FC = () => {
  const [files, setFiles] = useState<File[]>([]);

  return (
    <div>
      <UploadAndShow onFilesSelected={setFiles} />
    </div>
  );
}

export default App;
