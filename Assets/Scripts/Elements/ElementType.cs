using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

// This class is used to create new element objects depending on a given type and coordinates
public static class ElementTypes {

    // Method to create elements using matrix coordinates using a given element type enum type
    public static Element CreateElementByMatrix(this ElementType elementType, int x, int y, Vector3 velocity) {
        switch (elementType) {

            case ElementType.EMPTYCELL: return EmptyCell.GetInstance();
            case ElementType.STONE: return new Stone(x, y);
            case ElementType.SAND: return new Sand(x, y, velocity);
            case ElementType.WATER: return new Water(x, y, velocity);
            case ElementType.LAVA: return new Lava(x, y, velocity);
            case ElementType.PLAYERSEGMENT: return new PlayerSegment(x, y);
            case ElementType.PARTICLE: throw new InvalidOperationException();

            default:
                throw new Exception("Unsupported element type");
        }
    }
    // Currently unused but may use in the future, method to create the unused particles
    public static Element CreateParticleByMatrix(ElementMatrix matrix, int x, int y, Vector3 vector3, Element sourceElement) {
        if(matrix.isWithinBounds(x, y)) {
            Element newElement = new Particle(matrix, x, y, vector3, sourceElement);
            matrix.SetElementAtIndex(x, y, newElement);
            return newElement;
        }
        return null;
    }
}
// Enum for the different element types
public enum ElementType
{
    EMPTYCELL,
    STONE,
    SAND,
    WATER,
    LAVA,
    PLAYERSEGMENT,
    PARTICLE
}
