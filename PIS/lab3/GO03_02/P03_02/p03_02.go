package P03_02

import "fmt"

type Stats struct {
	getCount  int
	postCount int
}

func (s *Stats) PlusGet() {
	s.getCount++
}

func (s *Stats) PlusPost() {
	s.postCount++
}

func (s *Stats) GenStr() string {
	return fmt.Sprintf("Get-request count = %d, Post-request count = %d", s.getCount, s.postCount)
}
