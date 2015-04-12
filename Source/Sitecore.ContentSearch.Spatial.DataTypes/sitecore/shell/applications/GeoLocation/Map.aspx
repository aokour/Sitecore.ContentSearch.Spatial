<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Map.aspx.cs" Inherits="Sitecore.ContentSearch.Spatial.DataTypes.sitecore.shell.applications.GeoLocation.Map" %>
<!DOCTYPE html>
<html>
  <head>
    <title>Map</title>
    <meta name="viewport" content="initial-scale=1.0, user-scalable=no">
    <meta charset="utf-8">
    <style>
      html, body, #map-canvas {
        height: 100%;
        margin: 0px;
        padding: 0px
      }
      #panel {
        position: absolute;
        top: 5px;
        left: 50%;
        margin-left: -180px;
        z-index: 5;
        background-color: #fff;
        padding: 5px;
        border: 1px solid #999;
      }
    </style>
    <script src="https://maps.googleapis.com/maps/api/js?v=3.exp"></script>
    <script>
        var map;
        var marker;
        var geocoder;
        var lat = <%= !string.IsNullOrEmpty(Request.Params["lat"])?Request.Params["lat"]:"null"%>;
        var lon = <%= !string.IsNullOrEmpty(Request.Params["lon"])?Request.Params["lon"]:"null"%>;
        if (lat != null && lon != null) {
            var latlng = new google.maps.LatLng(lat, lon);
            initMapWithCoordinates(latlng);
        } else {
            initMap();
        }

        function codeAddress(update) {
            var address = document.getElementById('geo_address').value;
            geocoder.geocode( { 'address': address}, function(results, status) {
                if (status == google.maps.GeocoderStatus.OK) {
                    map.setCenter(results[0].geometry.location);
                    if(update)
                    {
                        

                        if (marker == null) {
                            marker = new google.maps.Marker({
                                position: results[0].geometry.location,
                                map: map,
                                title: ''
                            });
                        } else {
                            marker.setPosition(results[0].geometry.location);
                        }
                        
                    
                        parent.document.getElementById('<%=Request.Params["ctrlid"]%>').value = results[0].geometry.location.lat() + ',' + results[0].geometry.location.lng();
                    }
                    
                    map.setZoom(15);
                   
                } else {
                    alert('Geocode was not successful for the following reason: ' + status);
                }
            });
        }

        function initMap() {
            function initializeNoLatLon() {
                var mapOptions = {
                    zoom: 4,
                    center: { lat: 51.481581, lng: 0}
                };
                geocoder = new google.maps.Geocoder();
                map = new google.maps.Map(document.getElementById('map-canvas'),
                    mapOptions);
 

                google.maps.event.addListener(map, 'click', function(event) {
                    if (marker == null) {
                        marker = new google.maps.Marker({
                            position: event.latLng,
                            map: map,
                            title: 'Your place is here!'
                        });
                    } else {
                        marker.setPosition(event.latLng);
                    }
                    parent.document.getElementById('<%=Request.Params["ctrlid"]%>').value = event.latLng.lat().toString() + ',' + event.latLng.lng().toString();
                });
            }

            google.maps.event.addDomListener(window, 'load', initializeNoLatLon); 
        }

        function initMapWithCoordinates(latlon) {

            geocoder = new google.maps.Geocoder();
            function initialize() {
                var mapOptions = {
                    zoom: 15,
                    center: latlng
                };

                map = new google.maps.Map(document.getElementById('map-canvas'),
                    mapOptions);

                if (marker == null) {
                    marker = new google.maps.Marker({
                        position: latlng,
                        map: map,
                        title: 'Your place is here!'
                    });
                } else {
                    marker.setPosition(event.latLng);
                }

                google.maps.event.addListener(map, 'click', function(event) {
                    if (marker == null) {
                        marker = new google.maps.Marker({
                            position: event.latLng,
                            map: map,
                            title: 'Your place is here!'
                        });
                    } else {
                        marker.setPosition(event.latLng);
                    }
                    parent.document.getElementById('<%=Request.Params["ctrlid"]%>').value = event.latLng.lat().toString() + ',' + event.latLng.lng().toString();
                });
            }

            google.maps.event.addDomListener(window, 'load', initialize);
        }
    </script>
  </head>
  <body>
    <form id="form1" runat="server">
        <div id="panel">
      <input id="geo_address" type="textbox" value="" placeholder="Enter your address">
      <input type="button" value="Find" onclick="codeAddress(false)"> 
            <input type="button" value="Find & Update Field" onclick="codeAddress(true)">
    </div>
    <div id="map-canvas" style="height:380px;></div>
    </form>
  </body>
</html>

