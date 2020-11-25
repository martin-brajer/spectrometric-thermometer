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
    spectrum = gaussian_curve_factory(
        amplitude=10, variance=np.sqrt(2), expectedValue=center, zero=2)
    left = Line(constant=1.8, slope=0.1)
    right = Line(constant=-10.3, slope=4.0)
    intersection = Line.Intersection(left, right)

    # Plot
    plt.figure('SpectraProcessor.FitGraphics')

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


def gaussian_curve_factory(amplitude, variance, expectedValue, zero):
    """ Create gauss curve function of given parameters.

    :param float amplitude: amplitude
    :param float variance: width (kind of)
    :param float expectedValue: center
    :param float zero: baseline
    :return: gaussian curve as a function of one free variable
    :rtype: function
    """
    def gaussian_curve(x):
        """ Fixed gauss curve.

        :param x: free variable
        :type x: np.array or float
        :return: function value
        :rtype: np.array or float
        """
        return amplitude * np.exp(-((x - expectedValue)**2) / (2 * variance**2)) + zero
    return gaussian_curve


def dashed(x, func, xMax=None, yMax=None, xText=None, yText=None):
    """ Draw "X" at coordinates (x, func(x)), then draw dotted vertical and horizontal
    lines connecting the "X" to the axes.

    :param float x: free variable value
    :param func: function or function value
    :type func: function or float
    :param float xMax: horizontal dotted line is extended to this limit
    :param float yMax: vertical dotted line is extended to this limit
    :param str xText: annotation at the vertical dotted line near the x-axis
    :param str yText: annotation at the horizontal dotted line near the y-axis
    """
    xShift, yShift = 0.1, 0.2  # Annotation relative position

    if callable(func):
        y = func(x)
    else:
        y = func

    if yMax is None:
        yMax = y
    if xMax is None:
        xMax = x
    plt.plot([x, x], [0, yMax], 'k:')  # Vertical
    plt.plot([0, xMax], [y, y], 'k:')  # Horizontal
    plt.plot([x], [y], 'kX')  # Intersection

    ax = plt.gca()
    if xText is not None:
        ax.annotate(xText, xy=(xMax + xShift, yShift))
    if yText is not None:
        ax.annotate(yText, xy=(xShift, yMax + yShift))


# ---CLASSES---


class Line():
    """ Represents a line function. """

    def __init__(self, constant, slope):
        self.constant = constant
        self.slope = slope

    def __call__(self, x):
        """ Find function values. """
        return self.slope * x + self.constant

    def inverse(self, y):
        """ Find inverse function values. """
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
        """ Return coordinates Tuple(x, y) of intersection point of the two Lines. """
        x = Line._XIntersection(line1, line2)
        return (x, line1(x))


# ---MAIN---
if __name__ == '__main__':
    main()
