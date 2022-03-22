# IG-Mesh


## A mesh processing library for Grasshopper (Rhino)

`IG-Mesh`, standing for either `igl-grasshopper mesh` or `intelligent and geometric mesh`, is a grasshopper plugin targeting at triangular surface mesh processing.

The library is developed for the general architecture, design, and fabrication community, hoping to provide a one-stop solution to resolve the long-lasting pain in mesh processing and operation in the [Rhino](https://www.rhino3d.com) \& [Grasshopper](https://www.grasshopper3d.com) eco-system.

## Download & Installation 
The most recent release version can be found in the release section.
The [project page](_blank) on [Food4Rhino](https://www.food4rhino.com/en) also holds major version of this library.


## Library credit

Many of the base functions are borrowed from the famous geometry processing library [libigl](https://libigl.github.io), and ported into C# environment through the [PInvoke](https://www.grasshopper3d.com/forum/topics/link-use-c-code-or-c-lib-with-new-gh-plugin) methods. 

The rest is developed by the author in `C++` and ported in the same manner.

The name of this library is also partially inspired by the [libigl](https://libigl.github.io). The author would like to pay his deepest gratitude to the library developer for the tool and the responsive Q\&A during the past years.


## Alpha phase and Use case collection
**This library is currently under the alpha phase for initial public test. To improve it one-step further, your contribution is needed.**

Please submit an issue and tell me what your mesh processing task is and what functions are missing.

I will add the corresponding functions to the library after evaluation.


## Licence
The library is released under the [MIT licence](./LICENCE.md).


---
Author: [Zhao Ma](https://beyond-disciplines.com)

If `IG-Mesh` contributes to an academic publication, cite it as:
```bib
@software{ig-mesh,
  title = {IG-Mesh},
  author = {Zhao Ma},
  url = {https://github.com/xarthurx/IG-Mesh},
  version = {0.0.1}
  year = {2022}
}
```
<!-- ## Compilation and Contribution -->
<!-- To compile, one need to download the [libigl](https://libigl.github.io) library, and compile this `.NET`-based library to generate the corresponding `.gha` and `.dll` file. -->




