import * as React from 'react';
import UploadAndShow from './Components/UploadAndShow';
import "./Components/UploadAndShow.css";
import "./styles.css";
import { useState } from 'react';
import UserManual from './Components/UserManual.jsx';

const App: React.FC = () => {
  const [files, setFiles] = useState<File[]>([]);

  return (
    <div>
      <UploadAndShow onFilesSelected={setFiles} />
        <UserManual />
    </div>
  );
}

export default App;
