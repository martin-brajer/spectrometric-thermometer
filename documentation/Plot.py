# -*- coding: utf-8 -*-
"""
Explanatory plot for SpectraProcessor.FitGraphics class.
"""

# --IMPORTS---
import numpy as np
import matplotlib.pyplot as plt
# import pandas as pd

# --CONSTANTS---

# ---FUNCTIONS---


def main():
    # Parameters
    xMin = 1
    center = 6

    # Curves
    spectrum = gaussCurve(
        amplitude=10, dispersionSquared=4, center=center, zero=2)
    left = Line(constant=1.8, slope=0.1)
    right = Line(constant=-10.3, slope=4.0)
    intersection = Line.Intersection(left, right)

    # Plot
    plt.figure('FitGraphics')

    x = np.linspace(xMin, 8, num=1000)
    plt.plot(x, spectrum(x), 'r-')

    x = np.linspace(0, 7, num=1000)
    plt.plot(x, left(x), 'k-')

    x = np.linspace(2, 6, num=1000)
    plt.plot(x, right(x), 'k-')

    plt.plot(*intersection, 'bo')

    # Add guidelines and annotation
    dashed(xMin, left, xText='WMin')
    dashed(center, left, yMax=13)
    dashed(right.inverse(0), right)
    dashed(right.inverse(spectrum(center)), right, xMax=6.5, yText='IMax')

    # Plot details
    plt.xlim(0, None)
    plt.ylim(0, None)
    plt.xlabel('Wavelength (a.u.)')
    plt.ylabel('Intensities (a.u.)')
    plt.show()


def gaussCurve(amplitude, dispersionSquared, center, zero):
    """ Return Gauss curve function of given parameters. """
    return lambda x: amplitude * np.exp(-((x - center)**2) / dispersionSquared) + zero


def dashed(x, func, xMax=None, yMax=None, xText=None, yText=None):
    """ Plot cross at (x, func(x)). Dotted vertical and horizontal line to plot axis.

    Optional parameters:\n
      xMax, yMax: Dotted lines extension up to given limit.\n
      xText, yText: Annotation near the given axis.
    """
    y = func(x)
    if yMax is None:
        yMax = y
    if xMax is None:
        xMax = x
    plt.plot([x, x], [0, yMax], 'k:')  # Vertical
    plt.plot([0, xMax], [y, y], 'k:')  # Horizontal
    plt.plot([x], [y], 'kX')  # Intersection

    ax = plt.gca()
    xShift, yShift = 0.1, 0.2
    if xText is not None:
        ax.annotate(xText, xy=(xMax + xShift, yShift))
    if yText is not None:
        ax.annotate(yText, xy=(xShift, yMax + yShift))


# ---CLASSES---


class Line():
    """ Represents line function. """

    def __init__(self, constant, slope):
        self.constant = constant
        self.slope = slope

    def __call__(self, x):
        """ Find function values. """
        return self.slope * x + self.constant

    def inverse(self, y):
        """ Find inversion function values. """
        return (y - self.constant) / self.slope

    def __str__(self):
        return "Line: y = ax + b, where a = {}, b = {}.".format(self.slope, self.constant)

    @staticmethod
    def _XIntersection(line1, line2):
        """ Return x-coordinate of intersection point of the two Lines. """
        return (line1.constant - line2.constant) / \
            (line2.slope - line1.slope)

    @staticmethod
    def Intersection(line1, line2):
        """ Return Tuple(x, y) of coordinates of intersection point of the two Lines. """
        x = Line._XIntersection(line1, line2)
        return (x, line1(x))


# ---MAIN---
if __name__ == '__main__':
    main()
