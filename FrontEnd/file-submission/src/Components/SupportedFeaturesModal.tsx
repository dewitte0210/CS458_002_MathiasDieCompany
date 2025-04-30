import React, { useState } from "react";
import { Modal} from "react-bootstrap";
// the supportedFeatures obj holds an array of out features and some info about them
import { supportedFeatures } from "../SupportedFeatures"
interface ParentModalProps {
  showModal: boolean;
  handleCloseModal: () => void;
}

const ParentModal: React.FC<ParentModalProps> = ({
  showModal,
  handleCloseModal,
}) => {
  
  // State for child modal
  const [childModalVisible, setChildModalVisible] = useState(false);
  const [selectedImage, setSelectedImage] = useState<{
    src: string;
    description: string;
    longDescription: string;
  } | null>(null);

  const handleImageClick = (image: {
    src: string;
    description: string;
    longDescription: string;
  }) => {
    setSelectedImage(image);
    setChildModalVisible(true);
  };

  const handleCloseChildModal = () => {
    setChildModalVisible(false);
    setSelectedImage(null);
  };

  return (
    <>
      {/* Parent Modal */}
          <Modal show={showModal && !childModalVisible} onHide={handleCloseModal}>
        <Modal.Header closeButton>
          <Modal.Title>Supported Features</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <div className="modal-image-container">
            {supportedFeatures.map((image, index) => (
              <div
                className="modal-individual-image-container"
                key={index}
                onClick={() => handleImageClick(image)}
              >
                <img className="modal-image" src={image.src} alt={image.alt} />
                <p className="modal-image-description">{image.description}</p>
              </div>
            ))}
          </div>
        </Modal.Body>
        {/*<Modal.Footer>*/}
        {/*  <button className="animated-button" onClick={handleCloseModal}>*/}
        {/*    <span>Close</span>*/}
        {/*    <span></span>*/}
        {/*  </button>*/}
        {/*</Modal.Footer>*/}
      </Modal>

      {/* Child Modal */}
      <Modal show={childModalVisible} onHide={handleCloseChildModal}>
        <Modal.Header closeButton>
          <Modal.Title>Feature Details</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          {selectedImage && (
            <>
              <p>{selectedImage.longDescription}</p>
            </>
          )}
        </Modal.Body>
        {/*<Modal.Footer>*/}
        {/*<button className="animated-button" onClick={handleCloseChildModal}>*/}
        {/*    <span>Close</span>*/}
        {/*    <span></span>*/}
        {/*  </button>*/}
        {/*</Modal.Footer>*/}
      </Modal>
    </>
  );
};

export default ParentModal;
