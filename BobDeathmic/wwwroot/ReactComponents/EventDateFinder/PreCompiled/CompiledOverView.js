"use strict";

function _instanceof(left, right) { if (right != null && typeof Symbol !== "undefined" && right[Symbol.hasInstance]) { return right[Symbol.hasInstance](left); } else { return left instanceof right; } }

function _typeof(obj) { if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

function _classCallCheck(instance, Constructor) { if (!_instanceof(instance, Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } }

function _createClass(Constructor, protoProps, staticProps) { if (protoProps) _defineProperties(Constructor.prototype, protoProps); if (staticProps) _defineProperties(Constructor, staticProps); return Constructor; }

function _possibleConstructorReturn(self, call) { if (call && (_typeof(call) === "object" || typeof call === "function")) { return call; } return _assertThisInitialized(self); }

function _assertThisInitialized(self) { if (self === void 0) { throw new ReferenceError("this hasn't been initialised - super() hasn't been called"); } return self; }

function _getPrototypeOf(o) { _getPrototypeOf = Object.setPrototypeOf ? Object.getPrototypeOf : function _getPrototypeOf(o) { return o.__proto__ || Object.getPrototypeOf(o); }; return _getPrototypeOf(o); }

function _inherits(subClass, superClass) { if (typeof superClass !== "function" && superClass !== null) { throw new TypeError("Super expression must either be null or a function"); } subClass.prototype = Object.create(superClass && superClass.prototype, { constructor: { value: subClass, writable: true, configurable: true } }); if (superClass) _setPrototypeOf(subClass, superClass); }

function _setPrototypeOf(o, p) { _setPrototypeOf = Object.setPrototypeOf || function _setPrototypeOf(o, p) { o.__proto__ = p; return o; }; return _setPrototypeOf(o, p); }

var OverViewCalendar =
    /*#__PURE__*/
    function (_React$Component) {
        _inherits(OverViewCalendar, _React$Component);

        function OverViewCalendar(props) {
            var _this;

            _classCallCheck(this, OverViewCalendar);

            _this = _possibleConstructorReturn(this, _getPrototypeOf(OverViewCalendar).call(this, props));
            console.log(props);
            return _this;
        }

        _createClass(OverViewCalendar, [{
            key: "render",
            value: function render() {
                if (this.props.editLink !== "") {
                    return React.createElement("div", {
                        className: "row"
                    }, React.createElement("div", {
                        className: "col-md-3 col-12 mb-4"
                    }, this.props.name), React.createElement("div", {
                        className: "col-md-3 col-6 mb-4"
                    }, React.createElement("a", {
                        className: "button",
                        href: this.props.voteLink
                    }, "Vote")), React.createElement("div", {
                        className: "col-md-3 col-6 mb-4"
                    }, React.createElement("a", {
                        className: "button",
                        href: this.props.editLink
                    }, "Edit")), React.createElement("div", {
                        className: "col-md-3 col-6 mb-4"
                    }, React.createElement("a", {
                        className: "button",
                        href: this.props.deleteLink
                    }, "Delete")));
                } else {
                    return React.createElement("div", {
                        className: "row"
                    }, React.createElement("div", {
                        className: "col-md-6 col-12 mb-4"
                    }, this.props.name), React.createElement("div", {
                        className: "col-md-3 col-6 mb-4"
                    }, React.createElement("a", {
                        className: "button",
                        href: this.props.voteLink
                    }, "Vote")));
                }
            }
        }]);

        return OverViewCalendar;
    }(React.Component);

var OverView =
    /*#__PURE__*/
    function (_React$Component2) {
        _inherits(OverView, _React$Component2);

        function OverView(props) {
            var _this2;

            _classCallCheck(this, OverView);

            _this2 = _possibleConstructorReturn(this, _getPrototypeOf(OverView).call(this, props));
            _this2.state = {
                data: []
            };
            return _this2;
        }

        _createClass(OverView, [{
            key: "componentWillMount",
            value: function componentWillMount() {
                var _this3 = this;

                var xhr = new XMLHttpRequest();
                xhr.open('get', this.props.url, true);

                xhr.onload = function () {
                    var data = JSON.parse(xhr.responseText);

                    _this3.setState({
                        data: data
                    });
                };

                xhr.send();
            }
        }, {
            key: "render",
            value: function render() {
                calendarNodes = "";
                console.log(this.state.data);

                if (this.state.data.calendars !== undefined) {
                    var calendarNodes = this.state.data.calendars.map(function (calendar) {
                        return React.createElement(OverViewCalendar, {
                            key: calendar.key,
                            deleteLink: calendar.deleteLink,
                            editLink: calendar.editLink,
                            name: calendar.name,
                            voteLink: calendar.voteLink,
                            chatUsers: calendar.chatUsers
                        });
                    });
                }

                return React.createElement("div", {
                    className: "OverView"
                }, React.createElement("a", {
                    className: "button",
                    href: this.state.data.addCalendarLink
                }, "Add Calendar"), React.createElement("br", null), React.createElement("br", null), calendarNodes);
            }
        }]);

        return OverView;
    }(React.Component);

ReactDOM.render(React.createElement(OverView, {
    url: "/Events/OverViewData"
}), document.getElementById('reactcontent'));