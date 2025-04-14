export const TRANSLATOR = {
    "Group1A1" : "Rectangle",
    "Group1A2" : "Radius Corner Rectangle",
    "Group1B1" : "Circle",
    "Group1B2" : "Oblong",
    "Group1C" : "Triangle",
    "Group2A1" : "Ellipse",
    "Group2A2" : "Bowtie",
    "Group3" : "Chamfered Corner",
    "Group4" : "Perimeter Feature",
    "Group5" : "Mitered Notch",
    "Group6" : "Radius Notch",
    "Group10" : "C-Spacer", 
    "Group17" : "Freehand",
};

export function translate(groupName: string) {
    return TRANSLATOR[groupName] ?? groupName;
}
