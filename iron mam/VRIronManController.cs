<!doctype html>
<html>
  <head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1"/>
    <script src="https://aframe.io/releases/1.5.0/aframe.min.js"></script>
  </head>
  <body>
    <a-scene>
      <!-- VR camera with gaze cursor -->
      <a-entity position="0 1.6 0">
        <a-camera cursor="fuse: true; fuseTimeout: 1000"
                  raycaster="objects: .target"></a-camera>
      </a-entity>

      <!-- floor -->
      <a-plane rotation="-90 0 0" width="20" height="20" color="#7BC8A4"></a-plane>

      <!-- target cube -->
      <a-box class="target" position="0 2 -4" color="#4CC3D9"
             event-set__enter="_event: mouseenter; color: red"
             event-set__leave="_event: mouseleave; color: #4CC3D9"
             onclick="this.setAttribute('color','green')">
      </a-box>

      <a-sky color="#ECECEC"></a-sky>
    </a-scene>
  </body>
</html>