import * as React from 'react';
import DragNdrop from './Components/DragNdrop';
import "./Components/drag-drop.css";
import { useState } from 'react';

const App: React.FC = () => {
  const [files, setFiles] = useState<File[]>([]);

  return (
    <div>
      <DragNdrop onFilesSelected={setFiles} width={400} height={400} />
    </div>
  );
}

export default App;