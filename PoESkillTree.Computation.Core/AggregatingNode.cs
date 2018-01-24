﻿using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public delegate NodeValue? NodeValueAggregator(IEnumerable<NodeValue?> values);

    public class AggregatingNode : ICalculationNode
    {
        private readonly INodeCollection _nodes;
        private readonly NodeValueAggregator _aggregator;
        private List<ICalculationNode> _subscribedNodes;

        public AggregatingNode(INodeCollection nodes, NodeValueAggregator aggregator)
        {
            _nodes = nodes;
            _aggregator = aggregator;
        }

        public NodeValue? Value
        {
            get
            {
                SubscribeIfRequired();
                return _aggregator(_nodes.Items.Select(n => n.Node.Value));
            }
        }

        public event EventHandler ValueChanged;

        public void Dispose()
        {
            if (_subscribedNodes != null)
            {
                _nodes.ItemsChanged -= FormNodesOnItemsChanged;
                UnsubscribeFromNodes();
            }
        }

        private void FormNodesOnItemsChanged(object sender, EventArgs args)
        {
            UnsubscribeFromNodes();
            SubscribeToNodes();
            OnValueChanged(sender, args);
        }

        private void SubscribeIfRequired()
        {
            if (_subscribedNodes == null)
            {
                _nodes.ItemsChanged += FormNodesOnItemsChanged;
                SubscribeToNodes();
            }
        }

        private void UnsubscribeFromNodes()
        {
            foreach (var node in _subscribedNodes)
            {
                node.ValueChanged -= OnValueChanged;
            }
            _subscribedNodes = new List<ICalculationNode>();
        }

        private void SubscribeToNodes()
        {
            _subscribedNodes = new List<ICalculationNode>();
            foreach (var item in _nodes.Items)
            {
                item.Node.ValueChanged += OnValueChanged;
                _subscribedNodes.Add(item.Node);
            }
        }

        private void OnValueChanged(object sender, EventArgs args)
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}