import * as React from 'react';
import DragNdrop from './Components/DragNdrop';
import "./Components/drag-drop.css";
import { useState } from 'react';

const App: React.FC = () => {
  const [files, setFiles] = useState<File[]>([]);

  return (
    <div>
      <DragNdrop onFilesSelected={setFiles} width={window.innerWidth / 2} height={window.innerHeight} />
    </div>
  );
}

export default App;