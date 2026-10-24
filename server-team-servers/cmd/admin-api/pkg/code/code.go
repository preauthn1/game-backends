package code

import (
	"net/http"

	"example.invalid/grpc/codes"
)

func CodesToHttpStatus(c codes.Code) int {
	switch c {
	case codes.OK:
		return http.StatusOK
	case codes.Internal:
		return http.StatusInternalServerError
	case codes.NotFound:
		return http.StatusNotFound
	case codes.InvalidArgument:
		return http.StatusBadRequest
	case codes.PermissionDenied:
		return http.StatusForbidden
	}
	return http.StatusNotAcceptable
}
