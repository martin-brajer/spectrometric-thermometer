# coding: utf-8
# Created on: 11/2020
# Author: Martin Brajer
# E-mail: martin.brajer@seznam.cz
"""
Documentation plots for `spectrometer-express
<https://github.com/martin-brajer/spectrometric-thermometer>`_ project.
"""

# --IMPORTS---
import numpy as np
import matplotlib.pyplot as plt
# import pandas as pd

# --CONSTANTS---
POINTS = 1000

# ---FUNCTIONS---


def main():
    SpectraProcessor_FitGraphics()


def SpectraProcessor_FitGraphics():
    """ Explanatory plot for SpectraProcessor.FitGraphics data class """
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

    x = np.linspace(xMin, 8, num=POINTS)
    plt.plot(x, spectrum(x), 'r-')

    x = np.linspace(0, 7, num=POINTS)
    plt.plot(x, left(x), 'k-')

    x = np.linspace(2, 6, num=POINTS)
    plt.plot(x, right(x), 'k-')

    plt.plot(*intersection, 'bo')

    # Add guidelines and annotation
    plt_dashed(xMin, left, xText='WMin')
    plt_dashed(center, left, yMax=13)
    plt_dashed(right.inverse(0), right)
    plt_dashed(right.inverse(spectrum(center)), right, xMax=6.5, yText='IMax')

    # Plot details
    plt.xlim(0, None)
    plt.ylim(0, None)
    plt.xlabel('Wavelength (a.u.)')
    plt.ylabel('Intensities (a.u.)')
    plt.show()


def gaussian_curve_factory(amplitude, variance, expectedValue, zero):
    """ Create gauss curve function of given parameters

    :param float amplitude: Amplitude
    :param float variance: Width (kind of)
    :param float expectedValue: Center
    :param float zero: Baseline
    :return: Gaussian curve as a function of one free variable
    :rtype: function(:class:`numpy.ndarray`)
    """
    def gaussian_curve(x):
        """ Fixed gauss curve

        :param x: Free variable
        :type x: :class:`numpy.ndarray` or float
        :return: Function value
        :rtype: np.array or float
        """
        return amplitude * np.exp(-((x - expectedValue)**2) /
                                  (2 * variance**2)) + zero
    return gaussian_curve


def plt_dashed(x, func, xMax=None, yMax=None, xText=None, yText=None):
    """ Highlights (x, func(x)) coordinate point

    Draw "X" at coordinates (`x`, `func(x)`), then draw dotted vertical
    and horizontal lines connecting the "X" to the axes

    :param x: Free variable
    :type x: float
    :param func: Function or function value
    :type func: function(float) or float
    :param xMax: Horizontal dotted line is extended to this limit
    :type xMax: float, optional
    :param yMax: Vertical dotted line is extended to this limit
    :type yMax: float, optional
    :param xText: Annotation at the vertical dotted line near the x-axis
    :type xText: str, optional
    :param yText: Annotation at the horizontal dotted line near the y-axis
    :type yText: str, optional
    """
    #: Annotation relative position
    xShift, yShift = 0.1, 0.2

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
    """ Represents a line function: `y = ax + b`

    :param constant: Constant term (`b`), defaults to 0
    :type constant: int, optional
    :param slope: Linear term (`a`), defaults to 0
    :type slope: int, optional
    """

    def __init__(self, constant=0, slope=0):
        self.constant = constant
        self.slope = slope

    def __call__(self, x):
        """ Find function values of this :class:`Line`

        :param x: Free variable
        :type x: :class:`numpy.ndarray`
        :return: Function value
        :rtype: :class:`numpy.ndarray`
        """
        return self.slope * x + self.constant

    def inverse(self, y):
        """ Find inverse function values of this :class:`Line`

        :param y: Original function value
        :type y: :class:`numpy.ndarray`
        :return: Original free variable
        :rtype: :class:`numpy.ndarray`
        """
        return (y - self.constant) / self.slope

    def __str__(self):
        return "Line: y = ax + b, where a = {}, b = {}.".format(
            self.slope, self.constant)

    @staticmethod
    def Intersection(line1, line2):
        """ Return coordinates of intersection point of
        the two :class:`Line` instances

        :param line1: First line
        :type line1: :class:`Line`
        :param line2: Second line
        :type line2: :class:`Line`
        :return: Coordinates of the intersection of the two lines
        :rtype: tuple
        """
        x = (line1.constant - line2.constant) / \
            (line2.slope - line1.slope)
        return (x, line1(x))


if __name__ == '__main__':
    main()
