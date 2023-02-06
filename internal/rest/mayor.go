package rest

import (
	"net/http"
	"time"

	"github.com/Coflnet/sky-mayor/internal/model"
	"github.com/Coflnet/sky-mayor/internal/mongo"
	"github.com/gin-gonic/gin"
)

// @Summary      Get the current mayor
// @Description  Returns the current mayor
// @Tags         Mayor
// @Accept       */*
// @Produce      json
// @Success      200  {object}  model.Candidate
// @Failure      400  {object}  nil
// @Failure      404  {object}  nil
// @Router       /mayor/current [get]
func getCurrentMayor(c *gin.Context) {
	c.Writer.Header().Set("Cache-Control", "public, max-age=300")
	electionPeriod, _ := mongo.GetCurrentElectionPeriod()
	if electionPeriod == nil {
		c.JSON(http.StatusNotFound, gin.H{"message": "election period not found"})
		return
	}
	c.JSON(http.StatusOK, electionPeriod.Winner)
}

// @Summary      Get the next mayor
// @Description  Returns the mayor with the most votes in the current election. If there is currently no election, this returns null.
// @Tags         Mayor
// @Accept       */*
// @Produce      json
// @Success      200  {object}  model.Candidate
// @Failure      400  {object}  nil
// @Failure      404  {object}  nil
// @Router       /mayor/next [get]
func getNextMayor(c *gin.Context) {
	c.Writer.Header().Set("Cache-Control", "public, max-age=300")

	lastVoting, err := mongo.GetLastVoting()
	if err != nil {
		c.JSON(http.StatusBadRequest, err)
	}
	if time.Since(lastVoting.Timestamp) > 5*time.Minute {
		c.JSON(http.StatusNotFound, nil)
		return
	}
	if lastVoting == nil {
		c.JSON(http.StatusNotFound, gin.H{"message": "last voting not found"})
		return
	}
	currentElectionPeriod, err := mongo.GetCurrentElectionPeriod()
	if err != nil {
		c.JSON(http.StatusBadRequest, err)
	}

	maxVote := lastVoting.Votes[0]
	for _, vote := range lastVoting.Votes {
		if vote.Votes > maxVote.Votes {
			maxVote = vote
		}
	}

	var nextWinner *model.Candidate
	for _, candidate := range currentElectionPeriod.Candidates {
		if candidate.Key == maxVote.MayorKey {
			nextWinner = candidate
		}
	}

	c.JSON(http.StatusOK, nextWinner)
}
