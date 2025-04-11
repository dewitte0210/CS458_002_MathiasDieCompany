import * as React from 'react';
import Home from './Home';
import {Routes, Route} from 'react-router-dom'
import PricingConfig from "./Components/PricingConfig.jsx";

const App: React.FC = () => {

  return (
    <div>
        <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/admin" element={<PricingConfig />} />
        </Routes>
    </div>
  );
}

export default App;
