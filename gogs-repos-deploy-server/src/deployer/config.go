package deployer

type Config struct {
	Server struct {
		Port string
	}
	Release struct {
		Root string
	}
	Target struct {
		Root string
		GSDir string
		SrcDir string
		ExtName string
	}
	Stop struct {
		Base string
		Commands []string
	}
	Start struct {
		Base string
		Commands []string
	}
	Notify struct {
		WebHook string
	}
}