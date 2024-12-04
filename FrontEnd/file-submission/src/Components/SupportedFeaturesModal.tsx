import React, { useState } from "react";
import { Modal} from "react-bootstrap";

interface ParentModalProps {
  showModal: boolean;
  handleCloseModal: () => void;
}

const ParentModal: React.FC<ParentModalProps> = ({
  showModal,
  handleCloseModal,
}) => {
  // Supported features
  const images = [
    {
      src: "/Grp1A.ico",
      alt: "Group1A",
      description: "Group 1A",
      longDescription:
        'Group 1A features are comprised of square and radius corner rectangles greater than 1/4" wide. This group is typically internal features and cavity perimeters.',
    },
    {
      src: "/Grp1B.ico",
      alt: "Group1B",
      description: "Group 1B",
      longDescription:
        'Group 1B features are comprised of ruled circles and full radius obrounds greater than 3/8" wide. Circle cutouts less than 1 5/8" diameter are generally punches.',
    },
    {
      src: "/Grp1C.ico",
      alt: "Group1C",
      description: "Group 1C",
      longDescription:
        'Group 1C features are comprised of mitered and radius corner triangles. Mitered triangles will typically cost more becaue of the hand grinding required to miter the cut edges correctly. Overall dimensions less than 3/8" should be reviewed.',
    },
    {
      src: "/Grp2A.ico",
      alt: "Group2A",
      description: "Group 2A",
      longDescription:
        'Group 2A features are comprised of ruled ellipses and bowties (for lack of a better word) greater than 1/2" wide. Consultation is required for anything smaller! Many times, custom punches are favored over these ruled features.',
    },
    {
      src: "/Grp3.ico",
      alt: "Group3",
      description: "Group 3",
      longDescription: 
        "Group 3 is comprised of chamfered corners.",
    },
    {
      src: "/Grp4.ico",
      alt: "Group4",
      description: "Group 4",
      longDescription:
        "Group 4 is comprised of common perimeter features like V and Corner notches.",
    },
    {
      src: "/Grp5.ico",
      alt: "Group5",
      description: "Group 5",
      longDescription:
        "Group 5 is compriesed of mitered notches (perimeter features).",
    },
    {
      src: "/Grp6.ico",
      alt: "Group6",
      description: "Group 6",
      longDescription:
        "Group 6 is comprised of radius notches (perimeter features) and D-Subs with no tabs.",
    },
  ];

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
      <Modal show={showModal} onHide={handleCloseModal}>
        <Modal.Header closeButton>
          <Modal.Title>Supported Features</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <div className="modal-image-container">
            {images.map((image, index) => (
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
        <Modal.Footer>
          <button className="animated-button" onClick={handleCloseModal}>
            <span>Close</span>
            <span></span>
          </button>
        </Modal.Footer>
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
        <Modal.Footer>
        <button className="animated-button" onClick={handleCloseChildModal}>
            <span>Close</span>
            <span></span>
          </button>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default ParentModal;
