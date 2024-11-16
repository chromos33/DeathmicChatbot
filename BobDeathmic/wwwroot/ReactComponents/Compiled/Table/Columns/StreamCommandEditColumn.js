"use strict";

function _instanceof(left, right) { if (right != null && typeof Symbol !== "undefined" && right[Symbol.hasInstance]) { return !!right[Symbol.hasInstance](left); } else { return left instanceof right; } }

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

function _classCallCheck(instance, Constructor) { if (!_instanceof(instance, Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } }

function _createClass(Constructor, protoProps, staticProps) { if (protoProps) _defineProperties(Constructor.prototype, protoProps); if (staticProps) _defineProperties(Constructor, staticProps); return Constructor; }

function _inherits(subClass, superClass) { if (typeof superClass !== "function" && superClass !== null) { throw new TypeError("Super expression must either be null or a function"); } subClass.prototype = Object.create(superClass && superClass.prototype, { constructor: { value: subClass, writable: true, configurable: true } }); if (superClass) _setPrototypeOf(subClass, superClass); }

function _setPrototypeOf(o, p) { _setPrototypeOf = Object.setPrototypeOf || function _setPrototypeOf(o, p) { o.__proto__ = p; return o; }; return _setPrototypeOf(o, p); }

function _createSuper(Derived) { var hasNativeReflectConstruct = _isNativeReflectConstruct(); return function _createSuperInternal() { var Super = _getPrototypeOf(Derived), result; if (hasNativeReflectConstruct) { var NewTarget = _getPrototypeOf(this).constructor; result = Reflect.construct(Super, arguments, NewTarget); } else { result = Super.apply(this, arguments); } return _possibleConstructorReturn(this, result); }; }

function _possibleConstructorReturn(self, call) { if (call && (_typeof(call) === "object" || typeof call === "function")) { return call; } return _assertThisInitialized(self); }

function _assertThisInitialized(self) { if (self === void 0) { throw new ReferenceError("this hasn't been initialised - super() hasn't been called"); } return self; }

function _isNativeReflectConstruct() { if (typeof Reflect === "undefined" || !Reflect.construct) return false; if (Reflect.construct.sham) return false; if (typeof Proxy === "function") return true; try { Boolean.prototype.valueOf.call(Reflect.construct(Boolean, [], function () {})); return true; } catch (e) { return false; } }

function _getPrototypeOf(o) { _getPrototypeOf = Object.setPrototypeOf ? Object.getPrototypeOf : function _getPrototypeOf(o) { return o.__proto__ || Object.getPrototypeOf(o); }; return _getPrototypeOf(o); }

var StreamCommandEditColumn = /*#__PURE__*/function (_React$Component) {
  _inherits(StreamCommandEditColumn, _React$Component);

  var _super = _createSuper(StreamCommandEditColumn);

  function StreamCommandEditColumn(props) {
    var _this;

    _classCallCheck(this, StreamCommandEditColumn);

    _this = _super.call(this, props);
    _this.handleTypeClick = _this.handleTypeClick.bind(_assertThisInitialized(_this));
    _this.getTypeSwitchCSSClasses = _this.getTypeSwitchCSSClasses.bind(_assertThisInitialized(_this));
    _this.handleNameChange = _this.handleNameChange.bind(_assertThisInitialized(_this));
    _this.handleUptimeChange = _this.handleUptimeChange.bind(_assertThisInitialized(_this));
    _this.handleQuoteChange = _this.handleQuoteChange.bind(_assertThisInitialized(_this));
    _this.channelswitch = _this.channelswitch.bind(_assertThisInitialized(_this));
    _this.handleSave = _this.handleSave.bind(_assertThisInitialized(_this));
    _this.handleLoadEditForm = _this.handleLoadEditForm.bind(_assertThisInitialized(_this));
    _this.handleCancel = _this.handleCancel.bind(_assertThisInitialized(_this));
    _this.responsechange = _this.responsechange.bind(_assertThisInitialized(_this));
    _this.state = {
      Open: false,
      CanSave: true,
      Name: "",
      Response: "",
      Mode: "",
      StreamID: 0,
      StreamName: "",
      Streams: [],
      Change: false
    };
    return _this;
  }

  _createClass(StreamCommandEditColumn, [{
    key: "handleTypeClick",
    value: function handleTypeClick(e) {
      this.setState({
        Mode: e.target.getAttribute("data-type"),
        Change: true
      });
    }
  }, {
    key: "responsechange",
    value: function responsechange(e) {
      if (e.target.value === "") {
        this.setState({
          Response: e.target.value,
          CanSave: false,
          Change: true
        });
      } else {
        this.setState({
          Response: e.target.value,
          CanSave: true,
          Change: true
        });
      }
    }
  }, {
    key: "handleNameChange",
    value: function handleNameChange(e) {
      if (e.target.value === "") {
        this.setState({
          Name: e.target.value,
          CanSave: false,
          Change: true
        });
      } else {
        this.setState({
          Name: e.target.value,
          CanSave: true,
          Change: true
        });
      }
    }
  }, {
    key: "channelswitch",
    value: function channelswitch(e) {
      var index = e.nativeEvent.target.selectedIndex;
      this.setState({
        StreamID: this.state.Streams[index].ID,
        StreamName: this.state.Streams[index].Name,
        Change: true
      });
    }
  }, {
    key: "handleUptimeChange",
    value: function handleUptimeChange(e) {
      this.setState({
        AutoInterval: e.target.value,
        Change: true
      });
    }
  }, {
    key: "handleQuoteChange",
    value: function handleQuoteChange(e) {
      this.setState({
        Quote: e.target.value,
        Change: true
      });
    }
  }, {
    key: "getTypeSwitchCSSClasses",
    value: function getTypeSwitchCSSClasses(type) {
      var classes = "typeswitch";

      if (type === this.state.Mode) {
        classes += " active";
      }

      classes += " " + type;
      return classes;
    }
  }, {
    key: "handleSave",
    value: function handleSave(e) {
      if (this.state.CanSave && this.state.Change) {
        var request = new XMLHttpRequest();
        request.open("POST", "/StreamCommands/SaveCommand", true);
        var curthis = this;
        request.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        request.onreadystatechange = function () {
          // Call a function when the state changes.
          if (this.readyState === XMLHttpRequest.DONE && this.status === 200) {
            curthis.setState({
              Open: false,
              Change: false
            });
            window.dispatchEvent(new Event('updateTable'));
          }
        };

        var streamID = this.state.StreamID;

        if (streamID === 0) {
          streamID = this.state.Streams[0].ID;
        }

        request.send("ID=" + this.props.data.CommandID + "&Name=" + this.state.Name + "&Response=" + this.state.Response + "&Mode=" + this.state.Mode + "&StreamID=" + streamID + "&AutoInterval=" + this.state.AutoInterval);
      } else {
        this.setState({
          Open: false
        });
      }
    }
  }, {
    key: "handleCancel",
    value: function handleCancel(e) {
      this.setState({
        Open: false
      });
    }
  }, {
    key: "handleLoadEditForm",
    value: function handleLoadEditForm(e) {
      var xhr = new XMLHttpRequest();
      var thisreference = this;
      console.log(this.props);
      xhr.open('GET', "/StreamCommands/GetEditData?streamCommandID=" + this.props.data.CommandID, true);

      xhr.onload = function () {
        if (xhr.responseText !== "") {
          var data = JSON.parse(xhr.responseText);
          console.log(data);
          thisreference.setState({
            Open: true,
            Name: data.Name,
            Response: data.Response,
            Mode: data.Mode,
            StreamID: data.StreamID,
            StreamName: data.StreamName,
            Streams: data.Streams,
            AutoInterval: data.AutoInterval
          });
        }
      };

      xhr.send();
    }
  }, {
    key: "render",
    value: function render() {
      if (this.state.Open) {
        var options = this.state.Streams.map(function (e) {
          return /*#__PURE__*/React.createElement("option", {
            value: e.ID
          }, e.Name);
        });
        return /*#__PURE__*/React.createElement("td", null, this.props.data.Text, /*#__PURE__*/React.createElement("div", {
          className: "shadowlayer"
        }), /*#__PURE__*/React.createElement("div", {
          className: "statictest grid"
        }, /*#__PURE__*/React.createElement("label", {
          className: "namelabel"
        }, "Name"), /*#__PURE__*/React.createElement("input", {
          className: "namefield",
          name: "Name",
          value: this.state.Name,
          onChange: this.handleNameChange,
          type: "text"
        }), /*#__PURE__*/React.createElement("label", {
          className: "typelabel"
        }, "Type"), /*#__PURE__*/React.createElement("span", {
          "data-type": "Manual",
          onClick: this.handleTypeClick,
          className: this.getTypeSwitchCSSClasses("Manual")
        }, "Manual"), /*#__PURE__*/React.createElement("span", {
          "data-type": "Auto",
          onClick: this.handleTypeClick,
          className: this.getTypeSwitchCSSClasses("Auto")
        }, "Auto"), /*#__PURE__*/React.createElement("span", {
          "data-type": "Random",
          onClick: this.handleTypeClick,
          className: this.getTypeSwitchCSSClasses("Random")
        }, "Random"), this.state.Mode === "Auto" && /*#__PURE__*/React.createElement("div", {
          className: "autointerval"
        }, /*#__PURE__*/React.createElement("label", {
          className: "intervallabel"
        }, "Auto Interval"), /*#__PURE__*/React.createElement("input", {
          value: this.state.AutoInterval,
          onChange: this.handleUptimeChange,
          className: "intervalfield",
          type: "number"
        })), /*#__PURE__*/React.createElement("label", {
          className: "streamlabel"
        }, "Stream"), /*#__PURE__*/React.createElement("select", {
          className: "streamselect",
          value: this.state.StreamID,
          onChange: this.channelswitch
        }, options), /*#__PURE__*/React.createElement("label", {
          className: "responselabel"
        }, "Text"), /*#__PURE__*/React.createElement("textarea", {
          className: "responsefield",
          value: this.state.Response,
          onChange: this.responsechange
        }), /*#__PURE__*/React.createElement("span", {
          onClick: this.handleSave,
          className: "btn btn_primary savebtn"
        }, "Speichern"), /*#__PURE__*/React.createElement("span", {
          onClick: this.handleCancel,
          className: "btn btn_primary cancelbtn"
        }, "Abbrechen")));
      } else {
        return /*#__PURE__*/React.createElement("td", {
          className: "pointer",
          onClick: this.handleLoadEditForm
        }, this.props.data.Text);
      }
    }
  }]);

  return StreamCommandEditColumn;
}(React.Component);