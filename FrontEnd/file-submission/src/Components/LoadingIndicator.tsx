import * as React from 'react';

/*
  Shows a loading spinner and message during file upload.
*/
const LoadingIndicator: React.FC = () => (
  <>
    <span className="loader"></span>
    <div className="loading-text">Uploading...</div>
  </>
);

export default LoadingIndicator;
