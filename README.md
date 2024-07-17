CIF file converter
==================

CSharp application for converting Caltech Intermediate Format (CIF) files to PNG images. CIF is a possible export
option in Electric VLSI for exporting layouts.

Read [INSTALL](INSTALL) for build/install instructions.


Usage
-----

Generate PNG image containing all layers:
```
cifconv --png output.png input.cif
```


Example
-------

Cell in Electric VLSI:

![](input_example.png)

Can be exported by choosing "File"->"Export"->"CIF (Caltech Intermediate Format)..." from the menu. Then running cifconv
will result in the following PNG image.

```
cifconv --png dff_reg_bit_a.png dff_reg_bit_a.cif --width 400 --bg ffc0c0c0
```

![](output_example.png)

