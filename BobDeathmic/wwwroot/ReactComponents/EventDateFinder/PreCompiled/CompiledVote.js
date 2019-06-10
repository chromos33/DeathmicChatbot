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

var Calendar =
    /*#__PURE__*/
    function (_React$Component) {
        _inherits(Calendar, _React$Component);

        function Calendar(props) {
            var _this;

            _classCallCheck(this, Calendar);

            _this = _possibleConstructorReturn(this, _getPrototypeOf(Calendar).call(this, props));
            _this.state = {
                data: [],
                eventEmitter: new EventEmitter()
            };
            return _this;
        }

        _createClass(Calendar, [{
            key: "componentWillMount",
            value: function componentWillMount() {
                var thisreference = this;
                $.ajax({
                    url: "/Events/GetEventDates/" + this.props.ID,
                    type: "GET",
                    data: {},
                    success: function success(result) {
                        console.log(result);
                        thisreference.setState({
                            data: result
                        });
                    }
                });
            }
        }, {
            key: "render",
            value: function render() {
                if (this.state.data.Header !== undefined && this.state.data.Header.length > 0) {
                    var tempthis = this;
                    var headerNodes = this.state.data.Header.map(function (Header) {
                        return React.createElement(EventDate, {
                            key: Header.Date + Header.Time,
                            Data: Header
                        });
                    });
                    return React.createElement("div", {
                        className: "EventDateContainer"
                    }, headerNodes);
                } else {
                    return React.createElement("span", null, "Loading");
                }
            }
        }]);

        return Calendar;
    }(React.Component);

var EventDate =
    /*#__PURE__*/
    function (_React$Component2) {
        _inherits(EventDate, _React$Component2);

        function EventDate(props) {
            var _this2;

            _classCallCheck(this, EventDate);

            _this2 = _possibleConstructorReturn(this, _getPrototypeOf(EventDate).call(this, props));
            _this2.state = {
                Data: _this2.props.Data
            };
            return _this2;
        }

        _createClass(EventDate, [{
            key: "render",
            value: function render() {
                var tmpthis = this;
                var key = 0;
                var requestnodes = this.state.Data.Requests.map(function (request) {
                    key++;
                    console.log(request);
                    return React.createElement("div", {
                        className: "row usernode mr-0 ml-0"
                    }, React.createElement("span", {
                        className: "col-6 pt-2 pb-2"
                    }, request.UserName), React.createElement(StateSelect, {
                        key: key,
                        canEdit: request.canEdit,
                        requestID: request.AppointmentRequestID,
                        possibleStates: request.States,
                        state: request.State
                    }));
                });

                if (requestnodes.length > 0) {
                    return React.createElement("div", {
                        key: this.key,
                        className: "EventDate"
                    }, React.createElement("div", {
                        className: "row ml-0 mr-0"
                    }, React.createElement("span", {
                        className: "col-12 text-center bg_dark"
                    }, React.createElement("span", {
                        className: "date"
                    }, this.state.Data.Date), React.createElement("br", null), React.createElement("span", {
                        className: "time"
                    }, this.state.Data.Time))), requestnodes);
                } else {
                    return React.createElement("div", {
                        key: this.key,
                        className: "VoteUser"
                    }, React.createElement("span", {
                        className: "VoteUser_Name"
                    }, this.props.Name));
                }
            }
        }]);

        return EventDate;
    }(React.Component);

var StateSelect =
    /*#__PURE__*/
    function (_React$Component3) {
        _inherits(StateSelect, _React$Component3);

        function StateSelect(props) {
            var _this3;

            _classCallCheck(this, StateSelect);

            _this3 = _possibleConstructorReturn(this, _getPrototypeOf(StateSelect).call(this, props));
            _this3.state = {
                possibleStates: props.possibleStates,
                State: props.state
            };
            _this3.handleOnChange = _this3.handleOnChange.bind(_assertThisInitialized(_this3));
            return _this3;
        }

        _createClass(StateSelect, [{
            key: "handleOnChange",
            value: function handleOnChange(event) {
                this.setState({
                    State: event.target.value
                });
                var thisreference = this;
                var tmpevent = event;
                $.ajax({
                    url: "/Events/UpdateRequestState/",
                    type: "GET",
                    data: {
                        requestID: thisreference.props.requestID,
                        state: event.target.value
                    },
                    success: function success(result) { }
                });
            }
        }, {
            key: "render",
            value: function render() {
                var tmpthis = this;

                if (this.props.canEdit) {
                    if (this.state.possibleStates.length > 0) {
                        var states = this.state.possibleStates.map(function (state) {
                            if (state === "NotYetVoted") {
                                if (tmpthis.state.State === 0) {
                                    return React.createElement("option", {
                                        value: "0"
                                    }, "Noch nicht entschieden");
                                } else {
                                    return React.createElement("option", {
                                        value: "0"
                                    }, "Noch nicht entschieden");
                                }
                            }

                            if (state === "Available") {
                                if (tmpthis.state.State === 1) {
                                    return React.createElement("option", {
                                        value: "1"
                                    }, "Ich kann");
                                } else {
                                    return React.createElement("option", {
                                        value: "1"
                                    }, "Ich kann");
                                }
                            }

                            if (state === "NotAvailable") {
                                if (tmpthis.state.State === 2) {
                                    return React.createElement("option", {
                                        value: "2"
                                    }, "Ich kann nicht");
                                } else {
                                    return React.createElement("option", {
                                        value: "2"
                                    }, "Ich kann nicht");
                                }
                            }

                            if (state === "IfNeedBe") {
                                if (tmpthis.state.State === 3) {
                                    return React.createElement("option", {
                                        value: "3"
                                    }, "Wenn es sein muss");
                                } else {
                                    return React.createElement("option", {
                                        value: "3"
                                    }, "Wenn es sein muss");
                                }
                            }
                        });
                        return React.createElement("span", {
                            "data-state": this.state.State,
                            className: "requestNode col-6 pt-2 pb-2"
                        }, React.createElement("div", null, React.createElement("select", {
                            key: this.props.key,
                            value: this.state.State,
                            onChange: this.handleOnChange,
                            className: "chatUser_" + this.props.key
                        }, states)));
                    }
                } else {
                    switch (this.state.State) {
                        case 0:
                            return React.createElement("span", {
                                className: "requestNode col-6 pt-2 pb-2",
                                "data-state": this.state.State
                            }, React.createElement("div", null, React.createElement("p", {
                                className: "mb-0"
                            }, "Noch nicht entschieden")));

                        case 1:
                            return React.createElement("span", {
                                className: "requestNode col-6 pt-2 pb-2",
                                "data-state": this.state.State
                            }, React.createElement("div", null, React.createElement("p", {
                                className: "mb-0"
                            }, "Ich kann")));

                        case 2:
                            return React.createElement("span", {
                                className: "requestNode col-6 pt-2 pb-2",
                                "data-state": this.state.State
                            }, React.createElement("div", null, React.createElement("p", {
                                className: "mb-0"
                            }, "Ich kann nicht")));

                        case 3:
                            return React.createElement("span", {
                                className: "requestNode col-6 pt-2 pb-2",
                                "data-state": this.state.State
                            }, React.createElement("div", null, React.createElement("p", {
                                className: "mb-0"
                            }, "Wenn es sein muss")));
                    }
                }

                return React.createElement("p", null, " No Users Loaded");
            }
        }]);

        return StateSelect;
    }(React.Component);