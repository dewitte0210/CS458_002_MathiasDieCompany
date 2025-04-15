export const TRANSLATOR = {
    "Group1A1" : "Rectangle",
    "Group1A2" : "Radius Corner Rectangle",
    "Group1B1" : "Circle",
    "Group1B2" : "Oblong",
    "Group1C" : "Triangle",
    "Group2A1" : "Ellipse",
    "Group2A2" : "Bowtie",
    "Group3" : "Chamfered Corner",
    "Group4" : "V Notch/Corner Notch",
    "Group5" : "Mitered Notch - 5",
    "Group6" : "Radius Notch - 6",
    "Group10" : "C-Spacer",
    "Group11": "Pac-Man",
    "Group12a": "Double D",
    "Group12b": "Cross-Hair",
    "Group17" : "Radius Notch - 17",
};

export function translate(groupName: string) {
    return TRANSLATOR[groupName] ?? groupName;
}
