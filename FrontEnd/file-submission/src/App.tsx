import * as React from 'react';
import DragNdrop from './Components/UploadAndShow';
import "./Components/drag-drop.css";
import { useState } from 'react';

const App: React.FC = () => {
  const [files, setFiles] = useState<File[]>([]);

  return (
    <div>
      <DragNdrop onFilesSelected={setFiles} />
    </div>
  );
}

export default App;
