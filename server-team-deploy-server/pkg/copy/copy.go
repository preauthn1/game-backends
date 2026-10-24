package copy

import (
	"fmt"
	"io/ioutil"
	"log"
	"os"
)

// File Copies file source to dest.
func File(source string, dest string) (err error) {

	input, err := ioutil.ReadFile(source)
	if err != nil {
		return err
	}

	if err := ioutil.WriteFile(dest, input, 0644); err != nil {
		return err
	}

	si, err := os.Stat(source)
	if err != nil {
		return err
	}

	os.Chmod(dest, si.Mode())

	return nil
}

// Dir Recursively copies a directory tree, attempting to preserve permissions.
// Source directory must exist, destination directory must *not* exist.
func Dir(source string, dest string) (err error) {

	// get properties of source dir
	fi, err := os.Stat(source)
	if err != nil {
		return fmt.Errorf("Find source dir \"%s\" error. %v", source, err)
	}

	if !fi.IsDir() {
		return fmt.Errorf("Source dir \"%s\" is not a directory", source)
	}

	// create dest dir
	if err := os.MkdirAll(dest, fi.Mode()); err != nil {
		return fmt.Errorf("Create destination dir \"%s\" error. %v", dest, err)
	}

	// read source dir
	entries, err := ioutil.ReadDir(source)
	if err != nil {
		return fmt.Errorf("Read source dir \"%s\" error. %v", source, err)
	}

	for _, entry := range entries {

		sfp := source + "/" + entry.Name()
		dfp := dest + "/" + entry.Name()

		if entry.IsDir() {
			if err := Dir(sfp, dfp); err != nil {
				log.Printf("Copy subdir \"%s\" error. %v\n", sfp, err)
			}
			continue
		}

		if err := File(sfp, dfp); err != nil {
			log.Printf("Copy file \"%s\" error. %v\n", sfp, err)
		}
	}

	return
}
