using System.Collections.Generic;

public class CharacterDefaultWrongNodes
{
    public Character character;
    public List<DiscussionNode> nodes = new List<DiscussionNode>();
}

public class UtilityNodesCollection
{    
    public List<DiscussionNode> wrongAnswer = new List<DiscussionNode>();

    public List<DiscussionNode> debateWrongEvidence = new List<DiscussionNode>();
    
    public List<CharacterDefaultWrongNodes> characterDefaultWrongNodes = new List<CharacterDefaultWrongNodes>();

    public List<DiscussionNode> gameOverNodes = new List<DiscussionNode>();
    }
